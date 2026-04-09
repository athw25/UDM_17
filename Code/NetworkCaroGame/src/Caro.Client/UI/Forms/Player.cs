using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro.Client.UI.Forms
{
    public class Player
    {
        private string name; // ctrl+R+E

        public string Name { get => name; set => name = value; }

        private Image mark;
        public Image Mark { get => mark; set => mark = value; }

        public Player(string name, Image mark)
        {
            this.name = name;
            this.mark = mark;
        }

    }
}
