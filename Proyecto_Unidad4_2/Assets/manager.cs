using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{
    public GameObject canvas, comArduino, comArduino2, comTouch;
    public void StartOn() {

        canvas.SetActive(false);
        comArduino.SetActive(true);
        comArduino2.SetActive(true);
        comTouch.SetActive(true);
    }
    public void StartOff() {

        canvas.SetActive(true);
        comArduino.SetActive(false);
        comArduino2.SetActive(false);
        comTouch.SetActive(false);
    }
}
