using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event EventHandler OnStart;

    public event EventHandler OnStop;

    public event EventHandler OnDone;

    public double TimeStart { get; private set; }

    public double TimeEnd { get; private set; }

    public double TimeElapsed => Time.time - TimeStart;

    public double TimeLeft => TimeEnd - Time.time;

    public bool IsRunning { get; private set; } = false;

    [SerializeField]
    private TextMeshProUGUI text;

    public void StartTimer(double duration)
    {
        if (IsRunning)
            throw new InvalidOperationException("Cannot start timer when already running.");

        TimeStart = Time.time;
        TimeEnd = TimeStart + duration;
        IsRunning = true;
        OnStart?.Invoke(this, null);
    }

    public void StopTimer()
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
            text.text = string.Format("{0:0.00}", TimeLeft);

        if (Time.time > TimeEnd)
        {
            IsRunning = false;
            OnDone?.Invoke(this, null);
            OnStop?.Invoke(this, null);

            if (text != null)
            {
                text.text = "GO";
                StartCoroutine(HideGo());
            }
        }
    }

    IEnumerator HideGo()
    {
        yield return new WaitForSeconds(1f);

        if (text != null)
            text.text = "";
    }
}
