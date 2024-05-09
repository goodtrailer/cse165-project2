using OVR.OpenVR;
using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

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

    // for direction
    private bool startedMovement = false;
    //private Vector3 initHandPos = Vector3.zero;
    //private Vector3 headsetPos = Vector3.zero;
    private Vector3 initOffset = Vector3.zero;

    private GameObject centerEyeAnchor;

    private LineRenderer controlVisual;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

        centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        controlVisual = GameObject.Find("ControlVisual").GetComponent<LineRenderer>();
    }

    void Start()
    {
        controlVisual.enabled = false;
    }

    void Update()
    {
        controlVisual.positionCount = 2;

        controlVisual.SetPositions(new[]
        {
            centerEyeAnchor.transform.position + initOffset,
            gestureRecognizer.transform.position,
        });
    }

    void FixedUpdate()
    {
        float coefficient = gestureRecognizer.GetSimilarity("Closed", 0.98f);

        
        if (coefficient > 0)
        {
            if (!startedMovement)
            {
                startedMovement = true;
                controlVisual.enabled = true;

                // headset offset
                initOffset = gestureRecognizer.transform.position - centerEyeAnchor.transform.position;

            }

            Vector3 origin = centerEyeAnchor.transform.position + initOffset;

            Direction = gestureRecognizer.transform.position - origin;

            if (Direction.magnitude < 0.1)
                Direction = Vector3.zero;
        }
        else
        {
            controlVisual.enabled = false;
            startedMovement = false;
            Direction = Vector3.zero;
        }

        rb.velocity = IsFlying ? Direction.normalized * coefficient * Speed : Vector3.zero;
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
    private const double no_fly_duration = 3.0;

    IEnumerator Die()
    {
        IsFlying = false;

        deathFade.FadeIn(fade_duration);
        yield return new WaitForSeconds((float)fade_duration);
        deathFade.FadeOut(fade_duration);

        transform.position = prevWaypoint;
        transform.rotation = prevRotation;

        yield return new WaitForSeconds((float)no_fly_duration);
        IsFlying = true;
    }
}
