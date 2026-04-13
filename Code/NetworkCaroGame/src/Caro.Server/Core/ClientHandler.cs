using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
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
        private readonly object _lock = new object();
        private CancellationTokenSource _cts;

        public PlayerInfo PlayerInfo { get; private set; }

        public event Action<ClientHandler, Packet> OnPacketReceived;
        public event Action<ClientHandler> OnDisconnected;

        public ClientHandler(TcpClient client)
        {
            _client = client;
            PlayerInfo = new PlayerInfo
            {
                Id = Guid.NewGuid().ToString()
            };
        }

        public void Start()
        {
            try
            {
                var stream = _client.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };

                _cts = new CancellationTokenSource();

                Task.Run(() => ReceiveLoop(_cts.Token));
                Console.WriteLine($"[Client {PlayerInfo.Id}] Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Start error: {ex.Message}");
                Disconnect();
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    string json = await _reader.ReadLineAsync();

                    // Client disconnect
                    if (json == null)
                        break;

                    try
                    {
                        var packet = Serializer.Deserialize<Packet>(json);

                        if (packet != null)
                        {
                            OnPacketReceived?.Invoke(this, packet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client {PlayerInfo.Id}] Deserialize error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client {PlayerInfo.Id}] Error: {ex.Message}");
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

                lock (_lock) // thread-safe
                {
                    _writer.WriteLine(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client {PlayerInfo.Id}] Send error: {ex.Message}");
                Disconnect();
            }
        }

        private bool _isDisconnected = false;

        public void Disconnect()
        {
            if (_isDisconnected) return;
            _isDisconnected = true;

            try
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }

                _reader?.Close();
                _writer?.Close();
                _client?.Close();
            }
            catch { }

            Console.WriteLine($"[Client {PlayerInfo.Id}] Disconnected");

            OnDisconnected?.Invoke(this);
        }
    }
}