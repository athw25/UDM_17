using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Client.Network
{
    public class ClientSocket
    {
        private static ClientSocket _instance;
        public static ClientSocket Instance => _instance ??= new ClientSocket();

        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        public event Action<Packet> OnPacketReceived;
        public event Action OnDisconnected;

        public bool Connect(string ip, int port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(ip, port);
                
                var stream = _client.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };

                Task.Run(ReceiveLoop);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
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
                        // Safely invoke on UI thread if needed context, but we will handle it in Forms using Invoke()
                        OnPacketReceived?.Invoke(packet);
                    }
                }
            }
            catch
            {
                // Disconnected
            }
            finally
            {
                Disconnect();
            }
        }

        public void Send(Packet packet)
        {
            if (_client == null || !_client.Connected) return;
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
            _client?.Close();
            OnDisconnected?.Invoke();
        }
    }
}
