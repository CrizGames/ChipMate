using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChipMate
{
    /// <summary>
    /// http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.3
    /// </summary>
    public class EmuKeyboard
    {
        public enum EmuKey : byte
        {
            Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine,
            A, B, C, D, E, F,
        }

        public static readonly Dictionary<Keys, EmuKey> LiterralKeyMap = new Dictionary<Keys, EmuKey>
        {
            { Keys.NumPad0, EmuKey.Zero },
            { Keys.NumPad1, EmuKey.One },
            { Keys.NumPad2, EmuKey.Two },
            { Keys.NumPad3, EmuKey.Three },
            { Keys.NumPad4, EmuKey.Four },
            { Keys.NumPad5, EmuKey.Five },
            { Keys.NumPad6, EmuKey.Six },
            { Keys.NumPad7, EmuKey.Seven },
            { Keys.NumPad8, EmuKey.Eight },
            { Keys.NumPad9, EmuKey.Nine },
            { Keys.A,       EmuKey.A },
            { Keys.B,       EmuKey.B },
            { Keys.C,       EmuKey.C },
            { Keys.D,       EmuKey.D },
            { Keys.E,       EmuKey.E },
            { Keys.F,       EmuKey.F },
        };

        public static readonly Dictionary<Keys, EmuKey> KeyMap = new Dictionary<Keys, EmuKey>
        {
            { Keys.X,     EmuKey.Zero },
            { Keys.D1,    EmuKey.One },
            { Keys.D2,    EmuKey.Two },
            { Keys.D3,    EmuKey.Three },
            { Keys.Q,     EmuKey.Four },
            { Keys.W,     EmuKey.Five },
            { Keys.E,     EmuKey.Six },
            { Keys.A,     EmuKey.Seven },
            { Keys.S,     EmuKey.Eight },
            { Keys.D,     EmuKey.Nine },
            { Keys.Y,     EmuKey.A },
            { Keys.C,     EmuKey.B },
            { Keys.D4,    EmuKey.C },
            { Keys.R,     EmuKey.D },
            { Keys.F,     EmuKey.E },
            { Keys.V,     EmuKey.F },
        };

        public static readonly Dictionary<Key, EmuKey> KeyMapWPF = new Dictionary<Key, EmuKey>
        {
            { Key.X,     EmuKey.Zero },
            { Key.D1,    EmuKey.One },
            { Key.D2,    EmuKey.Two },
            { Key.D3,    EmuKey.Three },
            { Key.Q,     EmuKey.Four },
            { Key.W,     EmuKey.Five },
            { Key.E,     EmuKey.Six },
            { Key.A,     EmuKey.Seven },
            { Key.S,     EmuKey.Eight },
            { Key.D,     EmuKey.Nine },
            { Key.Y,     EmuKey.A },
            { Key.C,     EmuKey.B },
            { Key.D4,    EmuKey.C },
            { Key.R,     EmuKey.D },
            { Key.F,     EmuKey.E },
            { Key.V,     EmuKey.F },
        };

        public HashSet<EmuKey> KeysDown { get; private set; } = new HashSet<EmuKey>();

        public Action<EmuKey> OnNextKeyPress;

        public EmuKeyboard()
        {

        }

        public void OnKeyDown(Key key)
        {
            if (KeyMapWPF.ContainsKey(key))
                KeysDown.Add(KeyMapWPF[key]);

        }

        public void OnKeyUp(Key key)
        {
            if (KeyMapWPF.ContainsKey(key))
                KeysDown.Remove(KeyMapWPF[key]);
        }

        public void Update()
        {
            if (KeysDown.Count > 0 && OnNextKeyPress != null)
            {
                OnNextKeyPress(KeysDown.First());
                OnNextKeyPress = null;
            }

            // TODO: Make Xna Input work
            return;
            KeyboardState kstate = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            Keys[] keys = kstate.GetPressedKeys();

            KeysDown.Clear();

            foreach (Keys k in keys)
            {
                if (KeyMap.ContainsKey(k))
                {
                    Debug.WriteLine(KeyMap[k]);
                    KeysDown.Add(KeyMap[k]);

                    if (OnNextKeyPress != null)
                    {
                        OnNextKeyPress(KeyMap[k]);
                        OnNextKeyPress = null;
                    }
                }
            }
        }

        public bool IsKeyDown(EmuKey key)
        {
            return KeysDown.Contains(key);
        }
    }
}
