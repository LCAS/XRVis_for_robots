using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CustomAnchorManager : MonoBehaviour
{
    private ARAnchorManager arAnchorManager;

    void Awake()
    {
        // Get the ARAnchorManager component from the GameObject
        arAnchorManager = GetComponent<ARAnchorManager>();

        if (arAnchorManager == null)
        {
            Debug.LogError("-> ARAnchorManager component not found on this GameObject.");
        }
    }

    void OnEnable()
    {
        if (arAnchorManager != null)
        {
            arAnchorManager.anchorsChanged += OnAnchorsChanged;
        }
    }

    void OnDisable()
    {
        if (arAnchorManager != null)
        {
            arAnchorManager.anchorsChanged -= OnAnchorsChanged;
        }
    }

    private void OnAnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
    {
        // Handle anchor changes
        Debug.Log("CustomAnchorManager: Anchors changed");
    }

    public void SetAnchorFromROSData(Vector3 position, Quaternion rotation)
    {
        if (arAnchorManager == null) return;

        // Create a Pose from position and rotation
        Pose pose = new Pose(position, rotation);

        // Add a new anchor at the specified pose
        ARAnchor anchor = arAnchorManager.AddAnchor(pose);

        if (anchor != null)
        {
            Debug.Log("Anchor set from ROS data at: " + position);
        }
        else
        {
            Debug.LogWarning("Failed to create anchor from ROS data.");
        }
    }

    public void RemoveAnchor(ARAnchor anchor)
    {
        if (arAnchorManager != null)
        {
            Destroy(anchor.gameObject);
        }
    }
}
