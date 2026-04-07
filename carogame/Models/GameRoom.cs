using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace carogame.Models
{
    public class GameRoom
    {
        public string RoomId { get; private set; }
        public Player PlayerA { get; private set; }
        public Player PlayerB { get; private set; }

        public GameRoom(Player a, Player b)
        {
            RoomId = Guid.NewGuid().ToString();
            PlayerA = a;
            PlayerB = b;
        }
    }
}
