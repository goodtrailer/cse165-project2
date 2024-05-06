using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Fade : MonoBehaviour
{

    private double start = double.NegativeInfinity;
    private double duration = 1.0;
    private bool isFadingIn = false;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void FadeIn(double duration)
    {
        isFadingIn = true;
        start = Time.time;
        this.duration = duration;
    }

    public void FadeOut(double duration)
    {
        isFadingIn = false;
        start = Time.time;
        this.duration = duration;
    }


    void Update()
    {
        Color color = image.color;
        color.a = Mathf.Clamp01((float)((Time.time - start) / duration));
        if (!isFadingIn)
            color.a = 1f - color.a;

        color.a = 1 - Mathf.Pow(1 - color.a, 5);

        image.color = color;
    }
}
