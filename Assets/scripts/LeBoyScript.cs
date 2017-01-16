using LeBoyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

#region MicroStopwatch class

public class MicroStopwatch : System.Diagnostics.Stopwatch
{
  readonly double _microSecPerTick =
      1000000D / System.Diagnostics.Stopwatch.Frequency;

  public MicroStopwatch()
  {
    if (!System.Diagnostics.Stopwatch.IsHighResolution)
    {
      throw new Exception("On this system the high-resolution " +
                          "performance counter is not available");
    }
  }

  public long ElapsedMicroseconds
  {
    get
    {
      return (long)(ElapsedTicks * _microSecPerTick);
    }
  }
}

#endregion

#region LeBoy

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LeBoyInputScript))]
public class LeBoyScript : MonoBehaviour
{
  [Header("ROM")]
  public string romName;

  private SpriteRenderer spriteRenderer;
  private Texture2D emulatorBackBuffer;
  private GBZ80 emulator;

  private LeBoyInputScript input;
  private bool emulate;
  private Thread emulatorThread;

  void Awake()
  {
    // Preare render
    spriteRenderer = GetComponent<SpriteRenderer>();
    input = GetComponent<LeBoyInputScript>();

    emulatorBackBuffer = new Texture2D(160, 144, TextureFormat.RGB24, false, false);
    spriteRenderer.sprite = Sprite.Create(emulatorBackBuffer, new Rect(0, 0, emulatorBackBuffer.width, emulatorBackBuffer.height), Vector2.one * 0.5f);

    // Create emulator
    emulator = new GBZ80();

    // Load ROM
    Debug.Log("Loading " + romName + " ...");
    using (FileStream fs = new FileStream(Path.Combine(Application.streamingAssetsPath, romName), FileMode.Open))
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

    emulate = true;
    emulatorThread = new Thread(EmulatorWork);
    emulatorThread.Start();
  }

  void OnDestroy()
  {
    if (emulatorThread != null)
    {
      emulate = false;
    }
  }

  private void EmulatorWork()
  {
    double cpuSecondsElapsed = 0.0f;

    MicroStopwatch s = new MicroStopwatch();
    s.Start();

    while (emulate)
    {
      uint cycles = emulator.DecodeAndDispatch();

      // timer handling
      // note: there's nothing quite reliable / precise enough in cross-platform .Net
      // so this is quite hack-ish / dirty
      cpuSecondsElapsed += cycles / GBZ80.ClockSpeed;

      double realSecondsElapsed = s.ElapsedMicroseconds * 1000000;

      if (realSecondsElapsed - cpuSecondsElapsed > 0.0) // dirty wait
      {
        realSecondsElapsed = s.ElapsedMicroseconds * 1000000;
      }

      if (s.ElapsedMicroseconds > 1000000) // dirty restart every seconds to not loose too many precision
      {
        // TODO
        //s.Restart();
        cpuSecondsElapsed -= 1.0;
      }
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
      int WIDTH = 160;
      int HEIGHT = 144;
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

          // TODO BGRA
          emulatorBackBuffer.SetPixel(x, HEIGHT - y, new Color(r, g, b, a));
        }

      }
      emulatorBackBuffer.Apply();
    }

  }
}

#endregion