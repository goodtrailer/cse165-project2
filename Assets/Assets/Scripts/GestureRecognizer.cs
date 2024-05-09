using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
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

    public UnityEvent<bool> OnRecognized;

    [SerializeField]
    private List<Quaternion> data;

    public List<Quaternion> Data
    {
        get => data;

        set
        {
            data = value;
            float total = 0f;
            foreach (Quaternion d in data)
                total += d.x * d.x + d.y * d.y + d.z * d.z + d.w * d.w;
            Magnitude = Mathf.Sqrt(total);
        }
    }

    public void ComputeSimilarity(IList<Quaternion> other)
    {
        if (data == null || data.Count != other.Count)
        {
            Similarity = 0f;
            return;
        }

        float otherTotal = 0f;
        foreach (Quaternion d in other)
            otherTotal += d.x * d.x + d.y * d.y + d.z * d.z + d.w * d.w;
        float otherMagnitude = Mathf.Sqrt(otherTotal);

        float cosineSimilarity = 0f;
        for (int i = 0; i < other.Count; i++)
            cosineSimilarity += other[i].x * data[i].x + other[i].y * data[i].y + other[i].z * data[i].z + other[i].w * data[i].w;
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
    private OVRHand hand;

    void Awake()
    {
        skeleton = GetComponent<OVRSkeleton>();
        hand = GetComponent<OVRHand>();

        foreach (Gesture gesture in gestures)
            gestureNames.Add(gesture.Name, gesture);
    }

    public UnityEvent<bool> GetRecognizedEvent(string gestureName)
    {
        if (!gestureNames.ContainsKey(gestureName))
            return null;

        return gestureNames[gestureName].OnRecognized;
    }

    public float GetSimilarity(string gestureName, float threshold)
    {
        if (!hand.IsTracked || !gestureNames.ContainsKey(gestureName))
            return 0f;

        return Mathf.Clamp01((gestureNames[gestureName].Similarity - threshold) / (1f - threshold));
    }

    public Vector3 GetBonePosition(int bone)
    {
        if (skeleton.Bones.Count == 0)
            return Vector3.positiveInfinity;

        return skeleton.Bones[bone % skeleton.Bones.Count].Transform.position;
    }


    void Update()
    {
        if (!hand.IsTracked)
        {
            if (previousGesture != null)
                previousGesture.OnRecognized?.Invoke(false);
            previousGesture = null;
            return;
        }

        List<Quaternion> data = new List<Quaternion>();
        foreach (OVRBone bone in skeleton.Bones)
            data.Add(Quaternion.Inverse(skeleton.transform.rotation) *  bone.Transform.rotation);

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

        Gesture oldGesture = previousGesture;

        if (maxSimilarity > recognizeThreshold)
        {
            if (previousGesture != maxGesture)
            {
                if (previousGesture != null)
                    previousGesture.OnRecognized?.Invoke(false);
                maxGesture.OnRecognized?.Invoke(true);
            }
            previousGesture = maxGesture;
        }
        else
        {
            if (previousGesture != null)
                previousGesture.OnRecognized?.Invoke(false);
            previousGesture = null;
        }

        if (previousGesture != oldGesture)
            Debug.Log("Gesture recognized: " + (previousGesture?.Name ?? "null"));
    }
}
