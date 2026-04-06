using System;
using Caro.Server.Core;
using Caro.Shared.Network;

namespace Caro.Server.Services
{
    public class MatchmakingService
    {
        private ServerManager _server;

        public MatchmakingService(ServerManager server)
        {
            _server = server;
        }

        public void HandlePacket(ClientHandler client, Packet packet)
        {
            // TODO (Thành viên 2): Xử lý logic thách đấu ghép phòng ở đây
        }

        public void HandleDisconnect(ClientHandler client)
        {
            // TODO (Thành viên 2): Xử lý khi người chơi thoát
        }

        public void EndGame(GameRoom room)
        {
            // TODO (Thành viên 2): Xử lý giải phóng phòng khi kết thúc game
        }
    }
}
