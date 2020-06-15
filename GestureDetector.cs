using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Creating a structure to store our bone data
[System.Serializable]
public struct Gesture
{
    // Name of stored Pose
    public string name;

    // Bone vector space positional list
    public List<Vector3> fingerDatas;

    // Unity recognition event
    public UnityEvent onRecognized;

}
public class GestureDetector : MonoBehaviour
{
    // Creating our threshold for discarding gestures
    public float threshold = 0.05f;

    // Creating a hand skeleton reference 
    public OVRSkeleton skeleton;

    // Creating a list for stored Gestures
    public List<Gesture> gestures;

    // Creating a list of all bones in hand
    private List<OVRBone> fingerBones;

    // The gesture we just held
    private Gesture previousGesture;

    

    


    // Start is called before the first frame update
    void Start()
    {
        // Populating our bones list at start play
        fingerBones = new List<OVRBone>(skeleton.Bones);

        // Setting our last held gesture
        previousGesture = new Gesture();
        
    }

    // Update is called once per frame
    void Update()
    {
        // Updating bone data on tick
        fingerBones = new List<OVRBone>(skeleton.Bones);

        // When the space bar is pressed record gesture data
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }

        // Checking if we recognize a gesture on tick
        Gesture currentGesture = Recognize();
        bool hasRecognized = !currentGesture.Equals(new Gesture());

        // Checking if we are seeing a newly held gesture
        if(hasRecognized && !currentGesture.Equals(previousGesture))
        {
            // Display to console use of new gesture
            Debug.Log("New gesture used: " + currentGesture.name);

            // Setting our previous gesture to the current
            previousGesture = currentGesture;

            // Invoking unity onRecognized
            currentGesture.onRecognized.Invoke();
        }

    }

    // Funciton to save new hand gestures
    void Save()
    {
        // Creating a runtime Gesture
        Gesture g = new Gesture();

        // Naming our new Gesture
        g.name = "New Gesture";

        // Creating a list of all the vector 3 bone data
        List<Vector3> data = new List<Vector3>();

        // Populating our list
        foreach (var bone in fingerBones)
        {
            // Adding finger position data based on root hand relative data
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        // Add gestures and finger data
        g.fingerDatas = data;
        gestures.Add(g);
    }

    // Function to detect previously created gestures
    Gesture Recognize()
    {
        // Gesture to test
        Gesture currentGesture = new Gesture();

        // Creating probability detection
        float currentMin = Mathf.Infinity;

        // Comparing stored gestures
        foreach (var gesture in gestures)
        {
            float sumDistance = 0;

            // Should pose be discarded
            bool isDiscarded = false;

            // Getting bone data for comparison
            for (int i = 0; i < fingerBones.Count; i++)
            {
                // Storing finger data
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);

                // Checking distance
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);

                // Discarding unused getstures
                if(distance>threshold)
                {
                    isDiscarded = true;
                    break;
                }

                // If threshold is not under add to sumdistance
                sumDistance += distance;
            }

            // If not discarded set current gesture
            if(!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }

        return currentGesture;

    }
}
