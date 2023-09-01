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
    public struct Animation
    {
        // Current frame index on a [numberOfFramesX] x [numberOfFramesY] grid
        private int frameX;
        private int frameY;

        // Number of rows and cols in the animation
        private readonly int numberOfFramesX;
        private readonly int numberOfFramesY;

        // Number of total frames in the animation
        private readonly int numberOfFrames => numberOfFramesX + numberOfFramesY;

        // Animation (x, y) off set on a tile set
        private int animationOffsetX;
        private int animationOffsetY;
        
        // Single animation frame size
        private readonly int frameWidth;
        private readonly int frameHeight;

        // Animation speed
        private readonly float frameElapsedTime;
        private float currentElapsedTime;

        // Loop the animation or end when currentFrame == numberOfFrames
        private bool isLooping;

        // Index of the current frame of the animation
        private int currentFrame;

        /// <summary>
        /// Index of the current frame of the animation
        /// </summary>
        public int CurrentFrame
        {
            get { return this.currentFrame; }
            private set
            {
                this.currentFrame = value;

                if (isLooping)
                {
                    this.currentFrame %= this.numberOfFrames;
                }
                else if(this.currentFrame >= this.numberOfFrames)
                {
                    this.currentFrame = this.numberOfFrames - 1;
                }

                this.frameX = this.currentFrame % this.numberOfFramesX;
                this.frameY = this.currentFrame / this.numberOfFramesY;
            }
        }

        /// <summary>
        /// Source rectangle that represent the current frame on a tile set
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            this.animationOffsetX + this.frameWidth * this.frameX,
            this.animationOffsetY + this.frameHeight * this.frameY,
            this.frameWidth,
            this.frameHeight);

        /// <summary>
        /// An object that allows playing a 2D animation
        /// </summary>
        /// <param name="frameWidth">Single frame width</param>
        /// <param name="frameHeight">Single frame height</param>
        /// <param name="numberOfFramesX">Number of animation coloumns</param>
        /// <param name="numberOfFramesY">Number of animation rows</param>
        /// <param name="frameElapsedTime">Time for each frame in milliseconds</param>
        /// <param name="isLooping">[True] if return to frame [0] on animation end. [False] if stay of the last frame.</param>
        /// <param name="animationOffsetX">X offset on the tileset</param>
        /// <param name="animationOffsetY">Y offset on the tileset</param>
        /// <param name="startFrame">The index of the first frame of the animation</param>
        public Animation(int frameWidth, int frameHeight, int numberOfFramesX, int numberOfFramesY,
            float frameElapsedTime, bool isLooping = true, int animationOffsetX = 0, int animationOffsetY = 0, int startFrame = 0)
        {
            // Setup single frame size
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;

            // Setup animation frame count till loop
            this.numberOfFramesX = numberOfFramesX;
            this.numberOfFramesY = numberOfFramesY;

            // Setup frame position
            this.animationOffsetX = animationOffsetX;
            this.animationOffsetY = animationOffsetY;

            // Setup current frame
            this.currentFrame = 0;
            this.frameX = 0;
            this.frameY = 0;

            // Setup animation speed
            this.frameElapsedTime = frameElapsedTime;
            this.currentElapsedTime = 0f;

            // Setup loop and start frame
            this.isLooping = isLooping;
            this.CurrentFrame = startFrame;
        }

        /// <summary>
        /// Play the animation
        /// </summary>
        public void Update(GameTime gameTime)
        {
            this.currentElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if(this.currentElapsedTime > this.frameElapsedTime)
            {
                this.CurrentFrame++;
                this.currentElapsedTime = 0f;
            }
        }
    }
}
