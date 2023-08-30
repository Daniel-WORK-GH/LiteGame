using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lite.Graphics
{
    public class Screen
    {
        internal Game game { get; private set; }
        internal int Width { get; private set; }
        internal int Height { get; private set; }

        public Screen(Game game, int width, int height)
        {
            this.game = game;
            this.Width = width;
            this.Height = height;
        }
    }
}
