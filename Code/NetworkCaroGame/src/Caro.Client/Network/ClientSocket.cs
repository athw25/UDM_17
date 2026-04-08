using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace Caro.Client.Network { 
    public class ClientSocket
    {
        private TcpClient? _client;
        private StreamWriter? _writer;
        private ClientListener? _listener;

        // Chuyển tiếp sự kiện từ Listener ra ngoài để UI hứng
        public event Action<Packet>? OnPacketReceived;
        public event Action? OnDisconnected;

        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port); // Kết nối bất đồng bộ tới Server

            var stream = _client.GetStream();
            
            // AutoFlush = true giúp dữ liệu gửi đi ngay lập tức thay vì đọng ở buffer
            _writer = new StreamWriter(stream) { AutoFlush = true };
            
            // Khởi tạo Listener và truyền luồng đọc cho nó
            StreamReader reader = new StreamReader(stream);
            _listener = new ClientListener(reader);

            // Đăng ký nhận sự kiện từ Listener và đẩy thẳng ra ngoài
            _listener.OnPacketReceived += (packet) => OnPacketReceived?.Invoke(packet);
            _listener.OnDisconnected += () => 
            {
                Disconnect();
                OnDisconnected?.Invoke();
            };

            // Kích hoạt luồng lắng nghe chạy ngầm (không dùng await ở đây để tránh block)
            _ = _listener.StartListeningAsync();
        }

        public async Task SendAsync(Packet packet)
        {
            if (_client == null || !_client.Connected) return;

            // Đóng gói dữ liệu thành JSON
            string json = Serializer.Serialize(packet);
            
            // Gửi đi và kết thúc bằng ký tự xuống dòng (để Listener bên kia dùng ReadLineAsync đọc được)
            await _writer.WriteLineAsync(json);
        }

        public void Disconnect()
        {
            _writer?.Close();
            _client?.Close();
        }
    }
    }