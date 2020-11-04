using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.WpfCore.MonoGameControls;

namespace ChipMate
{
    public class Emulator : MonoGameViewModel
    {
        public static Emulator instance;

        public EmuDisplay display;
        public EmuInterpreter interpreter;

        private string romPath;

        public bool rgb = false;


        public override void Initialize()
        {
            base.Initialize();

            instance = this;

            display = new EmuDisplay(GraphicsDevice);
        }

        public void RunROM(string romPath)
        {
            this.romPath = romPath;

            display.Clear();
            interpreter = new EmuInterpreter(display);

            LoadFontset();

            LoadRom();
        }

        public override void Update(GameTime gameTime) // Updates are happening in 60hz
        {
            if (string.IsNullOrEmpty(romPath))
                return;

            // Exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            // RGB Color
            if (rgb)
                display.fgColor = MiscTools.HsvToRgb((float)gameTime.TotalGameTime.TotalSeconds * 100f, 1f, 1f);

            interpreter.Update();
        }

        public override void Draw(GameTime gameTime)
        {
            display.Draw();
        }

        private void LoadFontset()
        {
            for (int i = 0; i < EmuSprites.Chip8_Fontset.Length; i++)
            {
                interpreter.Memory[i] = EmuSprites.Chip8_Fontset[i];
            }
        }

        private void LoadRom()
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(romPath)))
            {
                int pos = 0;
                int idx = 0;
                int length = (int)reader.BaseStream.Length;
                while (pos < length)
                {
                    interpreter.Memory[EmuInterpreter.MemIdx_ProgramStart + idx] = reader.ReadByte();
                    pos += sizeof(byte);
                    idx++;
                }
            }
        }
    }
}
