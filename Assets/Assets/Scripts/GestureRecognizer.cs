using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Gesture
{
    [field: SerializeField]
    public string Name { get; set; }

    [field: SerializeField]
    public float Magnitude { get; private set; }

    [field: SerializeField]
    public float Similarity { get; private set; }

    public UnityEvent OnRecognized;

    [SerializeField]
    private List<Vector3> data;

    public List<Vector3> Data
    {
        get => data;

        set
        {
            data = value;
            float total = 0f;
            foreach (Vector3 d in data)
                total += d.sqrMagnitude;
            Magnitude = Mathf.Sqrt(total);
        }
    }

    public void ComputeSimilarity(IList<Vector3> other)
    {
        if (data == null || data.Count != other.Count)
        {
            Similarity = 0f;
            return;
        }

        float otherTotal = 0f;
        foreach (Vector3 d in other)
            otherTotal += d.sqrMagnitude;
        float otherMagnitude = Mathf.Sqrt(otherTotal);

        float cosineSimilarity = 0f;
        for (int i = 0; i < other.Count; i++)
            cosineSimilarity += Vector3.Dot(other[i], data[i]);
        cosineSimilarity /= otherMagnitude * Magnitude;

        Similarity = cosineSimilarity * 0.5f + 0.5f;
    }
}

[RequireComponent(typeof(OVRSkeleton))]
[System.Serializable]
public class GestureRecognizer : MonoBehaviour
{
    [SerializeField]
    private float recognizeThreshold;

    [SerializeField]
    private List<Gesture> gestures;

    private Dictionary<string, Gesture> gestureNames = new Dictionary<string, Gesture>();

    [SerializeField]
    private bool isSaving = false;

    private Gesture previousGesture = null;

    private OVRSkeleton skeleton;

    private void Start()
    {
        skeleton = GetComponent<OVRSkeleton>();

        foreach (Gesture gesture in gestures)
            gestureNames.Add(gesture.Name, gesture);
    }

    public float GetSimilarity(string gestureName, float threshold)
    {
        if (!gestureNames.ContainsKey(gestureName))
            return 0f;

        return Mathf.Clamp01((gestureNames[gestureName].Similarity - threshold) / (1f - threshold));
    }


    void Update()
    {
        if (skeleton.Bones.Count == 0)
            return;

        List<Vector3> data = new List<Vector3>();
        foreach (OVRBone bone in skeleton.Bones)
        {
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        if (Input.GetKeyDown(KeyCode.Space) && isSaving)
        {
            Gesture gesture = new Gesture();
            gesture.Data = data;
            gestures.Add(gesture);
        }

        float maxSimilarity = 0f;
        Gesture maxGesture = null;
        foreach (Gesture g in gestures)
        {
            g.ComputeSimilarity(data);
            // Debug.Log(g.Similarity);
            if (g.Similarity > maxSimilarity)
            {
                maxSimilarity = g.Similarity;
                maxGesture = g;
            }
        }

        if (maxSimilarity > recognizeThreshold)
        {
            if (previousGesture != maxGesture)
            {
                maxGesture.OnRecognized?.Invoke();
                Debug.Log("Gesture recognized: " + maxGesture.Name + ", " + maxGesture.Similarity);
            }
            previousGesture = maxGesture;
        }
        else
        {
            previousGesture = null;
        }
    }
}
