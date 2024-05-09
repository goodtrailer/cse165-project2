using UnityEngine;
using UnityEngine.Events;

public class Waypoint : MonoBehaviour
{
    public UnityEvent OnDestroyed;

    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
