namespace Caro.Client.Network { 
    public class ClientListener {
        private readonly StreamReader _reader;
        
        // Các sự kiện để báo cho UI biết khi có gói tin mới hoặc bị mất kết nối
        public event Action<Packet>? OnPacketReceived;
        public event Action? OnDisconnected;

        public ClientListener(StreamReader reader)
        {
            _reader = reader;
        }

        public async Task StartListeningAsync()
        {
            try
            {
                while (true)
                {
                    // Đọc từng dòng (chống dính gói tin) bất đồng bộ
                    string? jsonLine = await _reader.ReadLineAsync();
                    
                    // Nếu Server ngắt kết nối, luồng stream sẽ trả về null
                    if (jsonLine == null) break; 

                    // Mở gói dữ liệu
                    Packet receivedPacket = Serializer.Deserialize(jsonLine);
                    
                    // Bắn sự kiện (báo cho ClientSocket hoặc UI)
                    OnPacketReceived?.Invoke(receivedPacket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi Listener]: {ex.Message}");
            }
            finally
            {
                OnDisconnected?.Invoke();
            }
        }
    } }