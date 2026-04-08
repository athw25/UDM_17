
namespace Caro.Shared.Network
{
    public class Packet
    {
        public CommandType Command { get; set; }
        public string Payload { get; set; } = string.Empty;
        // Storing inner object as JSON string to easily parse to specific types later
        public Packet() { }
        public Packet(CommandType command, string payload = "")
        {            
            Command = command;
            Payload = payload;
        }
    }
}
