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

        public Rectangle CalculateDestinationRectangle()
        {
            Rectangle backbufferRectangle = this.game.GraphicsDevice.PresentationParameters.Bounds;
            float backbuffer_aspect = (float)backbufferRectangle.Width / (float)backbufferRectangle.Height;
            float screen_aspect = (float)this.Width / (float)this.Height;

            float rx = 0;
            float ry = 0;
            float rw = backbufferRectangle.Width;
            float rh = backbufferRectangle.Height;

            if (screen_aspect > backbuffer_aspect)
            {
                rh = rw / screen_aspect;
                ry = (backbufferRectangle.Height - rh) / 2f;
            }
            else if (screen_aspect < backbuffer_aspect)
            {
                rw = rh * screen_aspect;
                rx = (backbufferRectangle.Width - rw) / 2f;
            }

            Rectangle result = new Rectangle((int)rx, (int)ry, (int)rw, (int)rh);
            return result;
        }
    }
}
