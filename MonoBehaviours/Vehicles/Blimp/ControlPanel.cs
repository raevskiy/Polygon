using System;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanel : MonoBehaviour {

    public Action<PressedKeyCode[]> KeyPressed;

    void FixedUpdate ()
	{
        var pressedKeyCode = new List<PressedKeyCode>();

        if (Input.GetAxis("Jump") > 0)
        {
            pressedKeyCode.Add(PressedKeyCode.SpeedUpPressed);
        }
        if (Input.GetAxis("Crouch") > 0)
        {
            pressedKeyCode.Add(PressedKeyCode.SpeedDownPressed);
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            pressedKeyCode.Add(PressedKeyCode.ForwardPressed);
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            pressedKeyCode.Add(PressedKeyCode.BackPressed);
        }
        if (Input.GetAxis("Horizontal") > 0)
        {
            pressedKeyCode.Add(PressedKeyCode.TurnRightPressed);
        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            pressedKeyCode.Add(PressedKeyCode.TurnLeftPressed);
        }

        KeyPressed?.Invoke(pressedKeyCode.ToArray());
    }
}
