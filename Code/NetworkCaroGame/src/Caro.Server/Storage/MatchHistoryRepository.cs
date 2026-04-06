using System;
using System.Collections.Generic;

namespace Caro.Server.Storage
{
    public class MatchHistoryRepository
    {
        public void SaveMatch(string player1, string player2, string winner)
        {
            // TODO (Thành viên 6): Mở file txt/Database và lưu kết quả trận đấu Caro ở đây
        }

        public List<string> GetHistory()
        {
            // TODO (Thành viên 6): Load lịch sử từ file và trả về
            return new List<string>();
        }
    }
}
