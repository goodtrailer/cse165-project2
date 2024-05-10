using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    public event EventHandler OnStart;

    public event EventHandler OnStop;

    public double TimeStart { get; private set; }

    public double TimeElapsed => Time.time - TimeStart;

    public bool IsRunning { get; private set; } = false;

    [SerializeField]
    private TextMeshProUGUI text;

    public void StartStopwatch()
    {
        if (IsRunning)
            throw new InvalidOperationException("Cannot start timer when already running.");

        TimeStart = Time.time;
        IsRunning = true;
        OnStart?.Invoke(this, null);
    }

    public void StopStopwatch()
    {
        if (!IsRunning)
            throw new InvalidOperationException("Cannot stop timer when not running.");

        IsRunning = false;
        OnStop?.Invoke(this, null);
    }

    void Update()
    {
        if (!IsRunning)
            return;

        if (text != null)
            text.text = string.Format("{0:0.00}", TimeElapsed);
    }
}
