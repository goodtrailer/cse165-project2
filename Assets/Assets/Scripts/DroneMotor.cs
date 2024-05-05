using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneMotor : MonoBehaviour
{
    [field: SerializeField]
    public Vector3 Direction { get; set; } = Vector3.zero;

    [field: SerializeField]
    public float Speed { get; set; } = 3f;

    [field: SerializeField]
    public bool IsFlying { get; set; } = false;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private LayerMask waypointLayer;

    [SerializeField]
    private Texture fadeTexture;

    private Rigidbody rb;

    private Vector3 prevWaypoint;
    private Quaternion prevRotation = Quaternion.identity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        rb.velocity = IsFlying ? Direction * Speed : Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if ((groundLayer & (1 << other.gameObject.layer)) != 0)
        {
            StartCoroutine(Die());
        }

        if ((waypointLayer & (1 << other.gameObject.layer)) != 0)
        {
            prevWaypoint = transform.position;
            prevRotation = transform.rotation;
            Destroy(other.gameObject);
        }
    }

    private const double fade_duration = 0.5;
    private double deathTime = double.NegativeInfinity;

    IEnumerator Die()
    {
        IsFlying = false;
        deathTime = Time.time;

        yield return new WaitForSeconds((float)fade_duration);
        
        transform.position = prevWaypoint;
        transform.rotation = prevRotation;
    }

    void OnGUI()
    {
        if (Time.time - deathTime > 2f * fade_duration || Time.time - deathTime < 0f)
            GUI.color = new Color(1f, 1f, 1f, 0f);
        else if (Time.time - deathTime > fade_duration)
            GUI.color = new Color(1f, 1f, 1f, 1f - (float)((Time.time - deathTime - fade_duration) / fade_duration));
        else
            GUI.color = new Color(1f, 1f, 1f, (float)((Time.time - deathTime) / fade_duration));

        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fadeTexture);
    }
}
