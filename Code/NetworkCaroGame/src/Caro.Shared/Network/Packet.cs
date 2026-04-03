
namespace Caro.Shared.Network
{
    public class Packet
    {
        public CommandType Command { get; set; }
        public object Data { get; set; }
    }
}
