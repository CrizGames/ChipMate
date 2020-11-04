using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChipMate
{
    public class EmuSound
    {
        private DynamicSoundEffectInstance sound;

        private const int Samples = 3000;
        private const int SampleRate = 44100;
        private const int Frequency = 440;
        private float[,] buffer;
        private byte[] xBuffer;
        private double timer;

        public bool IsPlaying { get; private set; }

        public EmuSound()
        {
            return;
            sound = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
            sound.Volume = 0.5f;

            xBuffer = new byte[Samples * 4];
            buffer = new float[2, Samples];
        }

        public void Update()
        {
            return;
            while (sound.PendingBufferCount < 3) SubmitBuffer();
        }

        public void Play()
        {
            return;
            if (IsPlaying)
                return;

            sound.Play();
            IsPlaying = true;
        }

        public void Stop()
        {
            return;
            if (!IsPlaying)
                return;

            sound.Stop();
            IsPlaying = false;
        }

        private void SubmitBuffer()
        {
            FillBuffer();
            ConvertBuffer(buffer, xBuffer);
            sound.SubmitBuffer(xBuffer);
        }

        private void FillBuffer()
        {
            for (int i = 0; i < Samples; i++)
            {
                buffer[0, i] = (float)SineWave(timer, Frequency);
                buffer[1, i] = (float)SineWave(timer, Frequency);
                timer += 1f / SampleRate;
            }
        }

        private double SineWave(double time, double frequency)
        {
            return Math.Sin(time * 2 * Math.PI * frequency);
        }

        private static void ConvertBuffer(float[,] from, IList<byte> to)
        {
            const int sampleBytes = 2;
            var channels = from.GetLength(0);
            var samplesBuffer = from.GetLength(1);

            for (int i = 0; i < samplesBuffer; i++)
            {

                for (int j = 0; j < channels; j++)
                {

                    var floatSample = MathHelper.Clamp(from[j, i], -1.0f, 1.0f);
                    var shortSample =
                        (short)(floatSample >= 0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);
                    int index = i * channels * sampleBytes + j * sampleBytes;

                    if (!BitConverter.IsLittleEndian)
                    {
                        to[index] = (byte)(shortSample >> 8);
                        to[index + 1] = (byte)shortSample;
                    }
                    else
                    {
                        to[index] = (byte)shortSample;
                        to[index + 1] = (byte)(shortSample >> 8);
                    }

                }
            }
        }

    }
}
