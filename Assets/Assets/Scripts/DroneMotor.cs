using OVR.OpenVR;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody))]
public class DroneMotor : MonoBehaviour
{
    [field: SerializeField]
    public Vector3 Direction { get; set; } = Vector3.zero;

    [field: SerializeField]
    public float Speed { get; set; } = 3f;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private LayerMask waypointLayer;

    [SerializeField]
    private bool isDead = false;

    [SerializeField]
    private Fade deathFade;

    [SerializeField]
    private Timer deathTimer;

    [SerializeField]
    private Stopwatch lapStopwatch;

    private Rigidbody rb;

    private Vector3 prevWaypoint;
    private Quaternion prevRotation = Quaternion.identity;

    [SerializeField]
    private GestureRecognizer gestureRecognizer;

    [SerializeField]
    private int currentBone = 0;

    private bool isFlyingForward = false;
    private Vector3 forwardDirection = Vector3.zero;
    private TextMeshProUGUI forwardText;

    private bool isFlyingOmni = false;
    private Vector3 omniOriginOffset = Vector3.zero;
    private GameObject centerEyeAnchor;
    private LineRenderer omniVisual;

    private const double no_fly_duration = 3.5;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

        centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        omniVisual = GameObject.Find("OmniVisual").GetComponent<LineRenderer>();
        forwardText = GameObject.Find("ForwardText").GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        omniVisual.gameObject.SetActive(false);

        deathTimer.OnStart += (_, _) => isDead = true;
        deathTimer.OnDone += (_, _) =>
        {
            isDead = false;
            if (!lapStopwatch.IsRunning)
                lapStopwatch.StartStopwatch();
        };
        deathTimer.StartTimer(no_fly_duration);

        gestureRecognizer.GetRecognizedEvent("OK").AddListener(f =>
        {
            if (f)
            {
                isFlyingForward = !isFlyingForward;
                forwardDirection = centerEyeAnchor.transform.forward;
                forwardText.text = isFlyingForward ? "FORWARD MODE\n\n\n\n\n\n\n" : "";
            }
        });
    }

    void Update()
    {
        omniVisual.positionCount = 2;

        omniVisual.transform.position = centerEyeAnchor.transform.position + omniOriginOffset;
        omniVisual.SetPositions(new[]
        {
            Vector3.zero,
            gestureRecognizer.GetBonePosition(currentBone) - omniVisual.transform.position,
        });

        if (Input.GetKeyDown(KeyCode.A))
            currentBone++;
    }

    void FixedUpdate()
    {
        float lesserCoefficient = gestureRecognizer.GetSimilarity("Closed", 0.775f);
        float coefficient = gestureRecognizer.GetSimilarity("Closed", 0.825f);

        if (!isFlyingForward && lesserCoefficient > 0)
        {
            if (!isFlyingOmni)
            {
                isFlyingOmni = true;
                omniVisual.gameObject.SetActive(true);
                omniOriginOffset = gestureRecognizer.GetBonePosition(currentBone) - centerEyeAnchor.transform.position;
            }

            Vector3 origin = centerEyeAnchor.transform.position + omniOriginOffset;

            Direction = gestureRecognizer.GetBonePosition(currentBone) - origin;

            if (Direction.magnitude < 0.1f)
                Direction = Vector3.zero;
        }
        else
        {
            omniVisual.gameObject.SetActive(false);
            isFlyingOmni = false;
            Direction = Vector3.zero;
        }

        if (isFlyingForward)
            Direction = forwardDirection;

        rb.velocity = !isDead ? Direction.normalized * coefficient * Speed : Vector3.zero;
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
