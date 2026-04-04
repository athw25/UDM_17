using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Caro.Shared.Network;
using Caro.Shared.Models;
using Caro.Shared.Utils;

namespace Caro.Server.Core
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        
        public PlayerInfo PlayerInfo { get; set; }
        public event Action<ClientHandler, Packet> OnPacketReceived;
        public event Action<ClientHandler> OnDisconnected;

        public ClientHandler(TcpClient client)
        {
            _client = client;
            PlayerInfo = new PlayerInfo { Id = Guid.NewGuid().ToString() };
        }

        public void Start()
        {
            var stream = _client.GetStream();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream) { AutoFlush = true };
            
            Task.Run(ReceiveLoop);
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (_client.Connected)
                {
                    string json = await _reader.ReadLineAsync();
                    if (json == null) break;

                    var packet = Serializer.Deserialize<Packet>(json);
                    if (packet != null)
                    {
                        OnPacketReceived?.Invoke(this, packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client disconnected: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        public void SendPacket(Packet packet)
        {
            try
            {
                string json = Serializer.Serialize(packet);
                _writer.WriteLine(json);
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect() 
        {
            _client.Close();
            OnDisconnected?.Invoke(this);
        }
    }
}