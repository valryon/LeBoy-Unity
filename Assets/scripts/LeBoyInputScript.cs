using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeBoyInputScript : MonoBehaviour
{
  [Header("Inputs")]
  public KeyCode keyLeftArrow = KeyCode.LeftArrow;
  public KeyCode keyRightArrow = KeyCode.RightArrow;
  public KeyCode keyUpArrow = KeyCode.UpArrow;
  public KeyCode keyDownArrow = KeyCode.DownArrow;
  public KeyCode keyA = KeyCode.A;
  public KeyCode keyB = KeyCode.B;
  public KeyCode keyStart = KeyCode.Return;
  public KeyCode keySelect = KeyCode.Backspace;

  private bool ignoreKeys;

  void Update()
  {
    if (ignoreKeys == false)
    {
      Left = Input.GetKey(keyLeftArrow);
      Right = Input.GetKey(keyRightArrow);
      Up = Input.GetKey(keyUpArrow);
      Down = Input.GetKey(keyDownArrow);
      A = Input.GetKey(keyA);
      B = Input.GetKey(keyB);
      Start = Input.GetKey(keyStart);
      Select = Input.GetKey(keySelect);
    }
  }

  public void PressA()
  {
    A = true;
    ignoreKeys = true;
  }

  public void PressB()
  {
    B = true;
    ignoreKeys = true;
  }

  public void PressUp()
  {
    Up = true;
    ignoreKeys = true;
  }

  public void PressDown()
  {
    Down = true;
    ignoreKeys = true;
  }

  public void PressRight()
  {
    Right = true;
    ignoreKeys = true;
  }

  public void PressLeft()
  {
    Left = true;
    ignoreKeys = true;
  }

  public void PressStart()
  {
    Start = true;
    ignoreKeys = true;
  }

  public void PressSelect()
  {
    Select = true;
    ignoreKeys = true;
  }

  public void ReleaseKeys()
  {
    A = false;
    B = false;
    Up = false;
    Down = false;
    Left = false;
    Right = false;
    Start = false;
    Select = false;
    ignoreKeys = false;
  }

  public bool A { get; private set; }
  public bool B { get; private set; }
  public bool Up { get; private set; }
  public bool Down { get; private set; }
  public bool Left { get; private set; }
  public bool Right { get; private set; }
  public bool Start { get; private set; }
  public bool Select { get; private set; }

}
