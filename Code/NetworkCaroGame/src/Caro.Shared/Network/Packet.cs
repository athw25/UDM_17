
namespace Caro.Shared.Network
{
    public class Packet
    {
        public CommandType Command { get; set; }
        public string Payload { get; set; } // Storing inner object as JSON string to easily parse to specific types later

        public string Data { get; set; }

    }
}
