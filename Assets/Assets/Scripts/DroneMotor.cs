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
    public bool IsFlying { get; set; } = true;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private LayerMask waypointLayer;

    [SerializeField]
    private Fade deathFade;

    private Rigidbody rb;

    private Vector3 prevWaypoint;
    private Quaternion prevRotation = Quaternion.identity;

    [SerializeField]
    private GestureRecognizer gestureRecognizer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        float coefficient = gestureRecognizer.GetSimilarity("Closed", 0.98f);
        rb.velocity = IsFlying ? Direction * coefficient * Speed : Vector3.zero;
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

    IEnumerator Die()
    {
        // IsFlying = false;

        deathFade.FadeIn(fade_duration);
        yield return new WaitForSeconds((float)fade_duration);
        deathFade.FadeOut(fade_duration);

        transform.position = prevWaypoint;
        transform.rotation = prevRotation;
    }
}
