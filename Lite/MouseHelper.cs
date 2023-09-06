using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lite.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lite
{
    public static class MouseHelper
    {
        private static MouseState currentMouseState;
        private static MouseState previousMouseState;

        public static void Update()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }

        public static bool IsLeftClick()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        }

        public static bool IsRightClick()
        {
            return currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
        }

        public static Vector2 GetMouseScreenPosition(Screen screen)
        {
            // Get the size and position of the screen when stretched to fit into the game window (keeping the correct aspect ratio).
            Rectangle screenDestinationRectangle = screen.CalculateDestinationRectangle();
            
            // Get the position of the mouse in the game window backbuffer coordinates.
            Point mouseWindowPosition = currentMouseState.Position;

            // Get the position of the mouse relative to the screen destination rectangle position.
            float sx = mouseWindowPosition.X - screenDestinationRectangle.X;
            float sy = mouseWindowPosition.Y - screenDestinationRectangle.Y;

            // Convert the position to a normalized ratio inside the screen destination rectangle.
            sx /= (float)screenDestinationRectangle.Width;
            sy /= (float)screenDestinationRectangle.Height;

            // Multiply the normalized coordinates by the actual size of the screen to get the location in screen coordinates.
            float x = sx * (float)screen.Width;
            float y = sy * (float)screen.Height;

            return new Vector2(x, y);
        }

        public static Vector2 GetMouseWorldPosition(Screen screen, Camera camera)
        {
            // Create a viewport based on the game screen.
            Viewport screenViewport = new Viewport(0, 0, screen.Width, screen.Height);

            // Get the mouse pixel coordinates in that screen.
            Vector2 mouseScreenPosition = GetMouseScreenPosition(screen);

            // Create a ray that starts at the mouse screen position and points "into" the screen towards the game world plane.
            Ray mouseRay = CreateMouseRay(mouseScreenPosition, screenViewport, camera);

            // Plane where the flat 2D game world takes place.
            Plane worldPlane = new Plane(new Vector3(0, 0, 1f), 0f);

            // Determine the point where the ray intersects the game world plane.
            float? dist = mouseRay.Intersects(worldPlane);
            Vector3 ip = mouseRay.Position + mouseRay.Direction * dist.Value;

            // Send the result as a 2D world position vector.
            Vector2 result = new Vector2(ip.X, ip.Y);
            return result;
        }

        private static Ray CreateMouseRay(Vector2 mouseScreenPosition, Viewport viewport, Camera camera)
        {
            // Near and far points that will indicate the line segment used to define the ray.
            Vector3 nearPoint = new Vector3(mouseScreenPosition, 0);
            Vector3 farPoint = new Vector3(mouseScreenPosition, 1);

            // Convert the near and far points to world coordinates.
            nearPoint = viewport.Unproject(nearPoint, camera.Projection, camera.View, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, camera.Projection, camera.View, Matrix.Identity);

            // Determine the direction.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // Resulting ray starts at the near mouse position and points "into" the screen.
            Ray result = new Ray(nearPoint, direction);
            return result;
        }
    }

}
