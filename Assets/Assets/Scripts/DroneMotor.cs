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

    [SerializeField]
    private Timer deathTimer;

    private Rigidbody rb;

    private Vector3 prevWaypoint;
    private Quaternion prevRotation = Quaternion.identity;

    [SerializeField]
    private GestureRecognizer gestureRecognizer;

    [SerializeField]
    private int currentBone = 0;

    // for direction
    private bool startedMovement = false;
    //private Vector3 initHandPos = Vector3.zero;
    //private Vector3 headsetPos = Vector3.zero;
    private Vector3 initOffset = Vector3.zero;

    private GameObject centerEyeAnchor;
    private LineRenderer controlVisual;

    private const double no_fly_duration = 3.0;

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
        controlVisual.gameObject.SetActive(false);

        deathTimer.OnStart += (_, _) => IsFlying = false;
        deathTimer.OnDone += (_, _) => IsFlying = true;
        deathTimer.StartTimer(no_fly_duration);
    }

    void Update()
    {
        controlVisual.positionCount = 2;

        controlVisual.transform.position = centerEyeAnchor.transform.position + initOffset;
        controlVisual.SetPositions(new[]
        {
            Vector3.zero,
            gestureRecognizer.GetBonePosition(currentBone) - controlVisual.transform.position,
        });

        if (Input.GetKeyDown(KeyCode.A))
            currentBone++;
    }

    void FixedUpdate()
    {
        float coefficient = gestureRecognizer.GetSimilarity("Closed", 0.8f);

        if (coefficient > 0)
        {
            if (!startedMovement)
            {
                startedMovement = true;
                controlVisual.gameObject.SetActive(true);

                // headset offset
                
                initOffset = gestureRecognizer.GetBonePosition(currentBone) - centerEyeAnchor.transform.position;
            }

            Vector3 origin = centerEyeAnchor.transform.position + initOffset;

            Direction = gestureRecognizer.GetBonePosition(currentBone) - origin;

            if (Direction.magnitude < 0.1f)
                Direction = Vector3.zero;
        }
        else
        {
            controlVisual.gameObject.SetActive(false);
            startedMovement = false;
            Direction = Vector3.zero;
        }

        rb.velocity = IsFlying ? Direction.normalized * coefficient * Speed : Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if ((groundLayer & (1 << other.gameObject.layer)) != 0)
            StartCoroutine(Die());

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
        deathTimer.StartTimer(no_fly_duration);

        deathFade.FadeIn(fade_duration);
        yield return new WaitForSeconds((float)fade_duration);
        deathFade.FadeOut(fade_duration);

        transform.position = prevWaypoint;
        transform.rotation = prevRotation;
    }
}
