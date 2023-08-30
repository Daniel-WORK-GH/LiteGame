using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lite
{
    public static class KeyboardHelper
    {
        private static KeyboardState previousKeystate;
        private static KeyboardState currentKeystate;

        public static void Update()
        {
            previousKeystate = currentKeystate;
            currentKeystate = Keyboard.GetState();
        }

        public static bool IsClicked(Keys k)
        {
            return previousKeystate.IsKeyUp(k) && currentKeystate.IsKeyDown(k);
        }
    }
}
