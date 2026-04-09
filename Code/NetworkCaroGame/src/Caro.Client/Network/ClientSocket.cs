using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Client.Network
{
    public class ClientSocket
    {
        private static ClientSocket _instance;
        public static ClientSocket Instance => _instance ??= new ClientSocket();

        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;

        public event Action<Packet> OnPacketReceived;

        private ClientSocket() { }

        // ================= CONNECT =================
        public bool Connect(string ip, int port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(ip, port);
                _stream = _client.GetStream();

                StartReceive();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connect error: " + ex.Message);
                return false;
            }
        }

        // ================= SEND =================
        public void Send(Packet packet)
        {
            try
            {
                if (_stream == null) return;

                string json = Serializer.Serialize(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);

                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send error: " + ex.Message);
            }
        }

        // ================= RECEIVE =================
        private void StartReceive()
        {
            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        private void ReceiveLoop()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                        continue;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var packet = Serializer.Deserialize<Packet>(json);

                    OnPacketReceived?.Invoke(packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Receive error: " + ex.Message);
            }
        }

        // ================= DISCONNECT =================
        public void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch { }
        }
    }
}