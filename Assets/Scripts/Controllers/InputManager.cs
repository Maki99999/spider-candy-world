using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static bool PressingUse()
    {
        return Input.GetKey(KeyCode.E);
    }

    public static bool PressingConfirm()
    {
        return Input.GetKey(KeyCode.Return);
    }
}
