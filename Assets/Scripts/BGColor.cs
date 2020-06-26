using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGColor : MonoBehaviour
{
    public enum BackgroundColorMode
    {
        Solid,
        Cycle
    }

    private Camera camera;
    public BackgroundColorMode mode;
    public float brightness;
    public float colorShiftSpeed;
    float colorShiftProgress = 0;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (mode == BackgroundColorMode.Cycle)
        {
            camera.backgroundColor = Color.HSVToRGB(Mathf.Lerp(0, 1, colorShiftProgress), 1, brightness);
            colorShiftProgress += colorShiftSpeed * Time.deltaTime;
            if (colorShiftProgress >= 1)
                colorShiftProgress = 0;
        }
    }

    public void SetColor(Color newColor)
    {
        mode = BackgroundColorMode.Solid;
        camera.backgroundColor = newColor;
    }

    public void SetCycle()
    {
        mode = BackgroundColorMode.Cycle;
    }
}
