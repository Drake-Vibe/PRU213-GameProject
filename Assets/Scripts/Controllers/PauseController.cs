using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseController : MonoBehaviour
{
    public static bool isGamePaused { get; private set; } = false;

    public static void SetPause(bool pause)
    {
        isGamePaused = pause;
    }
}
