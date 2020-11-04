using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChipMate
{
    /// <summary>
    /// http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.4
    /// </summary>
    public class EmuDisplay
    {
        // Graphics
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;

        // Screen
        public const int ScreenWidth = 64, ScreenHeight = 32;

        private Texture2D bgTex;
        private Texture2D whitePixel;
        private float pixelSize;
        public Color bgColor = Color.Black;
        public Color fgColor = Color.White;

        private bool[] pixels = new bool[ScreenWidth * ScreenHeight];
        private HashSet<int> renderingPixels = new HashSet<int>();


        public EmuDisplay(GraphicsDevice graphicsDevice)
        {
            this.spriteBatch = new SpriteBatch(graphicsDevice);
            this.graphicsDevice = graphicsDevice;

            // Create white pixel texture
            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData(new Color[] { Color.White });

            bgTex = new Texture2D(graphicsDevice, 2, 1);
            bgTex.SetData(new Color[] { Color.White, Color.White });
        }

        public void Draw()
        {
            pixelSize = graphicsDevice.Viewport.Width / 64f;

            graphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(bgTex, Vector2.Zero, null, bgColor, 0f, Vector2.Zero, graphicsDevice.Viewport.Width / 2, SpriteEffects.None, 1f);

            // Draw pixels
            foreach (int p in renderingPixels)
            {
                int x = p % ScreenWidth;
                int y = (p - x) / ScreenWidth;
                spriteBatch.Draw(whitePixel, new Vector2(x, y) * pixelSize, null, fgColor, 0f, Vector2.Zero, pixelSize, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
        }

        public void Clear()
        {
            pixels = new bool[ScreenWidth * ScreenHeight];
            renderingPixels.Clear();
        }

        public bool SetPixel(int x, int y)
        {
            if (x < 0)
                x += ScreenWidth;
            else if (x >= ScreenWidth)
                x -= ScreenWidth;

            if (y < 0)
                y += ScreenHeight;
            else if (y >= ScreenHeight)
                y -= ScreenHeight;

            int pixelIdx = x + y * ScreenWidth;
            bool pixel = pixels[pixelIdx] = !pixels[pixelIdx];// XOR pixel (http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#Dxyn)

            if (pixel)
                renderingPixels.Add(pixelIdx);
            else if (!pixel)
                renderingPixels.Remove(pixelIdx);

            return !pixel;
        }
    }
}
