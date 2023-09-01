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
        private int frameX;
        private int frameY;
        private readonly int numberOfFramesX;
        private readonly int numberOfFramesY;
        private readonly int numberOfFrames => numberOfFramesX + numberOfFramesY;

        private int animationOffsetX;
        private int animationOffsetY;

        private readonly int frameWidth;
        private readonly int frameHeight;

        private readonly float frameElapsedTime;
        private float currentElapsedTime;

        private bool isLooping;

        private int currentFrame;

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

        public Rectangle Bounds => new Rectangle(
            this.animationOffsetX + this.frameWidth * this.frameX,
            this.animationOffsetY + this.frameHeight * this.frameY,
            this.frameWidth,
            this.frameHeight);

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
