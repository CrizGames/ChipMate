using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChipMate
{
    public class EmuInterpreter
    {
        #region Memory
        public byte[] Memory = new byte[0xFFF + 1]; // = 4096 = 4KB

        public const int MemIdx_InterperterEnd = 0x1FF; // 511
        public const int MemIdx_ProgramStart = 0x200; // 512
        public const int MemIdx_ProgramStartETI660 = 0x600; // 1536

        // Registers
        public byte[] V = new byte[16];
        public byte DT = 0, ST = 0;
        public int I = 0; // 16bit
        public int PC = MemIdx_ProgramStart; // 16bit
        public int SP { get { return Stack.Peek(); } } // 16bit
        public Stack<int> Stack = new Stack<int>(16); // 16 * 16bit
        #endregion

        private Random rnd = new Random();

        public EmuSound sound = new EmuSound();
        public EmuKeyboard keyboard = new EmuKeyboard();
        public EmuDisplay display;

        private bool paused = false;
        private int opu = 10; // Operations per update


        public EmuInterpreter(EmuDisplay display)
        {
            this.display = display;
        }

        public void Update()
        {
            keyboard.Update();

            if (!paused)
            {
                // Run code
                for (int i = 0; i < opu; i++)
                {
                    // Shift first byte to the left and add the first byte on the right
                    int opcode = Memory[PC] << 8 | Memory[PC + 1];
                    //Debug.WriteLine($"Executing opcode: {Convert.ToString(opcode, toBase: 16)}; PC: {PC}");
                    ExecuteInstruction(opcode);
                }

                UpdateTimers();
            }

            HandleSound();
        }

        private void UpdateTimers()
        {
            // Decrease timers
            if (DT > 0)
                DT -= 1;

            if (ST > 0)
                ST -= 1;
        }

        private void HandleSound()
        {
            sound.Update();
            // Sound
            if (ST > 0)
            {
                sound.Play();
            }
            else
            {
                sound.Stop();
            }
        }

        /// <summary>
        /// http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#3.1
        /// </summary>
        public void ExecuteInstruction(int opcode)
        {
            // Most hex codes are written with 4 digits for easier reading
            // You could also shorten these (i.e. 0x00F0 to 0xF0)

            NextOp();

            int addr = opcode & 0x0FFF; // (nnn) lowest 12 bit of opcode
            int nibble = opcode & 0x000F; // (n) lowest 4 bit of opcode
            int x = (opcode & 0x0F00) >> 8; // lowest 4 bit of high byte / left byte of opcode
            int y = (opcode & 0x00F0) >> 4; ; // upper 4 bit of low byte / right byte of opcode
            byte byte_ = (byte)(opcode & 0x00FF); // (byte) lowest 8 bits of opcode

            switch (opcode & 0xF000) // Get only first 4 bits
            {
                case 0x0000:
                    switch (opcode)
                    {
                        // CLS
                        case 0x00E0:
                            Emulator.instance.display.Clear();
                            break;
                        // RET
                        case 0x00EE:
                            PC = Stack.Pop();
                            break;
                    }
                    break;

                // JP addr
                case 0x1000:
                    PC = addr;
                    break;

                // CALL addr
                case 0x2000:
                    Stack.Push(PC);
                    PC = addr;
                    break;

                // SE Vx, byte
                case 0x3000:
                    if (V[x] == byte_)
                        NextOp();
                    break;

                // SNE Vx, byte
                case 0x4000:
                    if (V[x] != byte_)
                        NextOp();
                    break;

                // SE Vx, Vy
                case 0x5000:
                    if (V[x] == V[y])
                        NextOp();
                    break;

                // LD Vx, byte
                case 0x6000:
                    V[x] = byte_;
                    break;

                // ADD Vx, byte
                case 0x7000:
                    V[x] += byte_;
                    break;

                // Mostly logical ops
                case 0x8000:
                    switch (opcode & 0x000F)
                    {
                        // LD Vx, Vy
                        case 0x0:
                            V[x] = V[y];
                            break;

                        // OR Vx, Vy
                        case 0x1:
                            V[x] |= V[y];
                            break;

                        // AND Vx, Vy
                        case 0x2:
                            V[x] &= V[y];
                            break;

                        // XOR Vx, Vy
                        case 0x3:
                            V[x] ^= V[y];
                            break;

                        // ADD Vx, Vy
                        case 0x4:
                            int result = V[x] + V[y];
                            V[0xF] = (byte)(result > 255 ? 1 : 0);
                            V[x] = (byte)result;
                            break;

                        // SUB Vx, Vy
                        case 0x5:
                            V[0xF] = (byte)(V[x] > V[y] ? 1 : 0);
                            V[x] -= V[y];
                            break;

                        // SHR Vx {, Vy}
                        case 0x6:
                            V[0xF] = (byte)(V[x] & 0x01); // 0x01 = 0b0000_0001
                            V[x] /= 2;
                            break;

                        // SUBN Vx, Vy
                        case 0x7:
                            V[0xF] = (byte)(V[y] > V[x] ? 1 : 0);
                            V[x] = (byte)(V[y] - V[x]);
                            break;

                        // SHL Vx {, Vy}
                        case 0xE:
                            V[0xF] = (byte)((V[x] & 0x80) == 1 ? 1 : 0); // 0x80 = 0b1000_0000
                            V[x] *= 2;
                            break;
                    }
                    break;

                // SNE Vx, Vy
                case 0x9000:
                    if (V[x] != V[y])
                        NextOp();
                    break;

                // LD I, addr
                case 0xA000:
                    I = addr;
                    break;

                // JP V0, addr
                case 0xB000:
                    PC = addr + V[0];
                    break;

                // RND Vx, byte
                case 0xC000:
                    V[x] = (byte)(rnd.Next(0, 256) & byte_);
                    break;

                // DRW Vx, Vy, nibble
                case 0xD000:
                    V[0xF] = 0;
                    for (int spriteRow = 0; spriteRow < nibble; spriteRow++)
                    {
                        byte sprite = Memory[I + spriteRow];

                        for (int spriteCol = 0; spriteCol < 8; spriteCol++)
                        {
                            if ((sprite & 0x80) != 0) // 0x80 = 0b1000_0000
                                if (display.SetPixel(spriteCol + V[x], spriteRow + V[y]))
                                    V[0xF] = 1;

                            sprite <<= 1;
                        }
                    }
                    break;

                // Keyboard ops
                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        // SKP Vx
                        case 0x9E:
                            if (keyboard.IsKeyDown((EmuKeyboard.EmuKey)V[x]))
                                NextOp();
                            break;

                        // SKNP Vx
                        case 0xA1:
                            if (!keyboard.IsKeyDown((EmuKeyboard.EmuKey)V[x]))
                                NextOp();
                            break;
                    }
                    break;

                // Timer ops
                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        // LD Vx, DT
                        case 0x07:
                            V[x] = DT;
                            break;

                        // LD Vx, K
                        case 0x0A:
                            paused = true;
                            Debug.WriteLine("paused: " + paused);
                            keyboard.OnNextKeyPress = (key) =>
                            {
                                V[x] = (byte)key;
                                paused = false;
                            };

                            break;

                        // LD DT, Vx
                        case 0x15:
                            DT = V[x];
                            break;

                        // LD ST, Vx
                        case 0x18:
                            ST = V[x];
                            break;

                        // ADD I, Vx
                        case 0x1E:
                            I += V[x];
                            break;

                        // LD F, Vx
                        case 0x29:
                            I = V[x] * 5;
                            break;

                        // LD B, Vx
                        case 0x33:
                            Memory[I] = (byte)(V[x] % 100);
                            Memory[I + 1] = (byte)(Memory[I] / 10);
                            Memory[I + 2] = (byte)(V[x] % 10);
                            break;

                        // LD [I], Vx
                        case 0x55:
                            for (int i = 0; i < x; i++)
                                Memory[I + i] = V[i];
                            break;

                        // LD Vx, [I]
                        case 0x65:
                            for (int i = 0; i < x; i++)
                                V[i] = Memory[I + i];
                            break;
                    }
                    break;

                default:
                    throw new Exception("Couldn't read opcode: 0x" + Convert.ToString(opcode, toBase: 16));
            }
        }

        /// <summary>
        /// Adds 2 to PC
        /// </summary>
        private void NextOp()
        {
            PC += 2;
        }
    }
}
