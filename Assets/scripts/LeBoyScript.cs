using System;
using LeBoyLib;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LeBoy.Unity
{
    #region LeBoy

    public class LeBoyScript : MonoBehaviour
    {
        public const int WIDTH = 160;
        public const int HEIGHT = 144;

        [Header("Bindings")]
        public SpriteRenderer spriteRenderer;

        public LeBoyInputs input;
        public LeBoyAudioChannel audioChannel1;
        public LeBoyAudioChannel audioChannel2;
        public LeBoyAudioChannel audioChannel3;
        public LeBoyAudioChannel audioChannel4;

        [Header("ROM")]
        public string romName;

        private Texture2D emulatorBackBuffer;
        private GBZ80 emulator;
        private bool keepEmulatorRunning = false;

        public const int AUDIO_BUFFER_SIZE = GBZ80.SPUSampleRate * 2;
        private readonly short[] audioBuffer1 = new short[AUDIO_BUFFER_SIZE];
        private int bufferLength1 = 0;
        private readonly short[] audioBuffer2 = new short[AUDIO_BUFFER_SIZE];
        private int bufferLength2 = 0;
        private readonly short[] audioBuffer3 = new short[AUDIO_BUFFER_SIZE];
        private int bufferLength3 = 0;
        private readonly short[] audioBuffer4 = new short[AUDIO_BUFFER_SIZE];
        private int bufferLength4 = 0;

        private object channel1lock = new object();
        private object channel2lock = new object();
        private object channel3lock = new object();
        private object channel4lock = new object();

        private Thread emulatorThread;

        public bool IsOn => keepEmulatorRunning && emulator != null;

        void Awake()
        {
            // Prepare render
            emulatorBackBuffer = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false, false);
            spriteRenderer.sprite = Sprite.Create(emulatorBackBuffer,
                new Rect(0, 0, emulatorBackBuffer.width, emulatorBackBuffer.height), Vector2.one * 0.5f);

            // Create emulator
            emulator = new GBZ80();

            // Load ROM
            Debug.Log("Loading " + romName + " ...");
            using (FileStream fs = new FileStream(Path.Combine(Application.streamingAssetsPath, romName),
                FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] rom = new byte[fs.Length];
                    for (int i = 0; i < fs.Length; i++)
                    {
                        rom[i] = br.ReadByte();
                    }

                    emulator.Load(rom);
                }
            }

            Debug.Log("Loading done. Starting LeBoy.");

            keepEmulatorRunning = true;
            emulatorThread = new Thread(EmulatorWork);
            emulatorThread.Start();
        }

        void OnDestroy()
        {
            if (emulatorThread != null)
            {
                keepEmulatorRunning = false;
            }
        }

        void FixedUpdate()
        {
            emulator.JoypadState[0] = input.Right;
            emulator.JoypadState[1] = input.Left;
            emulator.JoypadState[2] = input.Up;
            emulator.JoypadState[3] = input.Down;
            emulator.JoypadState[4] = input.B;
            emulator.JoypadState[5] = input.A;
            emulator.JoypadState[6] = input.Select;
            emulator.JoypadState[7] = input.Start;

            // Upload backbuffer to texture
            byte[] backbuffer = emulator.GetScreenBuffer();
            if (backbuffer != null)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    for (int y = 0; y < HEIGHT; y++)
                    {
                        int i = x + (y * WIDTH);
                        i *= 4;
                        float b = backbuffer[i + 0] / 255f;
                        float g = backbuffer[i + 1] / 255f;
                        float r = backbuffer[i + 2] / 255f;
                        float a = backbuffer[i + 3] / 255f;

                        emulatorBackBuffer.SetPixel(x, HEIGHT - y, new Color(r, g, b, a));
                    }
                }

                emulatorBackBuffer.Apply();
            }

            // Sound
            lock (channel1lock)
            {
                if (bufferLength1 > 0)
                {
                    audioChannel1.SetAudioBuffer(audioBuffer1, bufferLength1);
                    bufferLength1 = 0;
                }
            }

            lock (channel2lock)
            {
                if (bufferLength2 > 0)
                {
                    audioChannel2.SetAudioBuffer(audioBuffer2, bufferLength2);
                    bufferLength2 = 0;
                }
            }

            lock (channel3lock)
            {
                if (bufferLength3 > 0)
                {
                    audioChannel3.SetAudioBuffer(audioBuffer3, bufferLength3);
                    bufferLength3 = 0;
                }
            }

            lock (channel4lock)
            {
                if (bufferLength4 > 0)
                {
                    audioChannel4.SetAudioBuffer(audioBuffer4, bufferLength4);
                    bufferLength4 = 0;
                }
            }
        }

        private void EmulatorWork()
        {
            double emulationElapsed = 0.0f;
            double lastElapsedTime = 0.0f;

            Stopwatch s = new Stopwatch();
            s.Start();

            while (keepEmulatorRunning)
            {
                // timer handling
                // note: there's nothing quite reliable / precise enough in cross-platform .NET
                // so this is quite hack-ish / dirty
                if (emulationElapsed <= 0.0f)
                {
                    uint cycles = emulator.DecodeAndDispatch();
                    emulationElapsed += (cycles * Stopwatch.Frequency) / GBZ80.ClockSpeed; // host cpu ticks elapsed

                    if (emulator.Channel1Samples > 0)
                    {
                        lock (channel1lock)
                        {
                            for (int i = 0; i < emulator.Channel1Samples; i++)
                            {
                                audioBuffer1[bufferLength1] = emulator.Channel1Buffer[i];
                                bufferLength1++;
                            }
                        }
                    }

                    if (emulator.Channel2Samples > 0)
                    {
                        lock (channel2lock)
                        {
                            for (int i = 0; i < emulator.Channel2Samples; i++)
                            {
                                audioBuffer2[bufferLength2] = emulator.Channel2Buffer[i];
                                bufferLength2++;
                            }
                        }
                    }

                    if (emulator.Channel3Samples > 0)
                    {
                        lock (channel3lock)
                        {
                            for (int i = 0; i < emulator.Channel3Samples; i++)
                            {
                                audioBuffer3[bufferLength3] = emulator.Channel3Buffer[i];
                                bufferLength3++;
                            }
                        }
                    }

                    if (emulator.Channel4Samples > 0)
                    {
                        lock (channel4lock)
                        {
                            for (int i = 0; i < emulator.Channel4Samples; i++)
                            {
                                audioBuffer4[bufferLength4] = emulator.Channel4Buffer[i];
                                bufferLength4++;
                            }
                        }
                    }

                    emulator.Channel1Samples = 0;
                    emulator.Channel2Samples = 0;
                    emulator.Channel3Samples = 0;
                    emulator.Channel4Samples = 0;
                }

                long elapsed = s.ElapsedTicks;
                emulationElapsed -= elapsed - lastElapsedTime;
                lastElapsedTime = elapsed;

                if (s.ElapsedTicks > Stopwatch.Frequency) // dirty restart every seconds to not loose too many precision
                {
                    s.Restart();
                    lastElapsedTime -= Stopwatch.Frequency;
                }
            }
        }
    }

    #endregion
}