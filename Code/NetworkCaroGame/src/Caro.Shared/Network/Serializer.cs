using System.Text.Json;

namespace Caro.Shared.Network
{
    public static class Serializer
    {
        public static string Serialize(Packet packet)
        {
            return JsonSerializer.Serialize(packet);
        }

        public static Packet Deserialize(string json)
        {
            return JsonSerializer.Deserialize<Packet>(json)?? new Packet(); // Trả về Packet rỗng nếu Deserialize thất bại
        }
    }
}