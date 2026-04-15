using System;
using System.Collections.Generic;
using Caro.Server.Core;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Server.Services
{
    // Main service responsible for matchmaking and game coordination
    public class MatchmakingService
    {
        private ServerManager _server;
        // Reference to server manager (used to find clients)

        private List<ClientHandler> _waitingPlayers = new List<ClientHandler>();
        // List of players waiting for random matchmaking

        private List<GameRoom> _rooms = new List<GameRoom>();
        // List of active game rooms
        
        // Constructor: inject server dependency
        public MatchmakingService(ServerManager server)
        {
            _server = server;
        }

        // Central method: handles all incoming packets from clients
        public void HandlePacket(ClientHandler client, Packet packet)
        {
            switch (packet.Command)
            {
                case CommandType.ChallengeRequest:
                    HandleFindMatch(client);// random matchmaking
                    break;
                case CommandType.Challenge:
                    HandleDirectChallenge(client, packet);// send challenge
                    break;
                case CommandType.Accept:
                    HandleChallengeAccept(client, packet);// accept challenge
                    break;
                case CommandType.Reject:
                    HandleChallengeReject(client, packet);// reject challenge
                    HandleFindMatch(client);// try finding another match
                    break;

                // In-game actions → forward to corresponding room
                case CommandType.Move:
                case CommandType.Chat:
                case CommandType.GameOver:
                case CommandType.TimerTick:
                case CommandType.TimeOut:
                    ForwardToRoom(client, packet);
                    break;
            }
        }

        //RANDOM MATCHMAKING
        private void HandleFindMatch(ClientHandler client)
        {
            Console.WriteLine($"{client.PlayerInfo.Name} is finding match...");

            // Prevent duplicate entries in waiting queue
            if (_waitingPlayers.Contains(client))
                return;

            // If there is already a waiting player → match them
            if (_waitingPlayers.Count > 0)
            {
                var opponent = _waitingPlayers[0];// get first waiting player
                _waitingPlayers.RemoveAt(0);// remove from queue

                // Create a new game room
                var room = new GameRoom(opponent, client, this);
                _rooms.Add(room);

                Console.WriteLine($"Match found: {opponent.PlayerInfo.Name} vs {client.PlayerInfo.Name}");

                room.StartGame();// start the game
            }
            else
            {
                // No opponent available → add to waiting queue
                _waitingPlayers.Add(client);
                Console.WriteLine($"{client.PlayerInfo.Name} is waiting...");
            }
        }

        //DIRECT CHALLENGE
        private void HandleDirectChallenge(ClientHandler client, Packet packet)
        {
            string targetName = packet.Payload;// target player name
            var targetClient = _server.GetClientByName(targetName);
            if (targetClient != null)
            {
                // Send challenge request to target player
                targetClient.SendPacket(new Packet { Command = CommandType.Challenge, Payload = client.PlayerInfo.Name });
            }
        }

        //ACCEPT CHALLENGE
        private void HandleChallengeAccept(ClientHandler client, Packet packet)
        {
            string challengerName = packet.Payload;// challenger name
            var challenger = _server.GetClientByName(challengerName);
            if (challenger != null)
            {
                // Create a new game room
                var room = new GameRoom(challenger, client, this);
                _rooms.Add(room);
                room.StartGame();// start the game
            }
        }

        //REJECT CHALLENGE
        private void HandleChallengeReject(ClientHandler client, Packet packet)
        {
            string challengerName = packet.Payload;
            var challenger = _server.GetClientByName(challengerName);
            if (challenger != null)
            {
                // Notify challenger about rejection
                challenger.SendPacket(new Packet { Command = CommandType.Reject, Payload = client.PlayerInfo.Name });
            }
        }

        // FORWARD GAME DATA
        private void ForwardToRoom(ClientHandler client, Packet packet)
        {
            var room = FindRoomByClient(client);// find the room of this client
            if (room != null)
            {
                // Delegate packet handling to the game room
                room.HandlePacket(client, packet);
            }
        }

        //HANDLE DISCONNECT 
        public void HandleDisconnect(ClientHandler client)
        {
            Console.WriteLine($"{client.PlayerInfo.Name} disconnected");
            _waitingPlayers.Remove(client);// remove from waiting queue

            var room = FindRoomByClient(client);
            if (room != null)
            {
                // Identify opponent
                var opponent = room.Player1 == client ? room.Player2 : room.Player1;

                // Notify opponent about disconnection
                opponent.SendPacket(new Packet
                {
                    Command = CommandType.PlayerDisconnected,
                    Payload = ""
                });

                _rooms.Remove(room);// remove the room
            }
        }

        //END GAME
        public void EndGame(GameRoom room)
        {
            Console.WriteLine("Ending game...");

            if (_rooms.Contains(room))
            {
                _rooms.Remove(room);// remove room from active list
            }
        }

        //FIND ROOM BY CLIENT
        private GameRoom FindRoomByClient(ClientHandler client)
        {
            foreach (var room in _rooms)
            {
                if (room.Player1 == client || room.Player2 == client)
                    return room;
            }
            return null;
        }
    }
}


