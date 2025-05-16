using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PauseRenderingController : MonoBehaviour {
    public static bool ShouldPauseRendering { get; set; }
    public void SetPauseRendering(string shouldPause)
    {
        Debug.Log("SetPauseRendering, " + shouldPause);
        ShouldPauseRendering = bool.Parse(shouldPause);
    }

    void Update() {
        if (ShouldPauseRendering) {
            Time.timeScale = 0;
        } else {
            Time.timeScale = 1;
        }
    }
}
