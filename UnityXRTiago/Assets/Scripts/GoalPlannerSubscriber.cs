using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using System.Collections.Generic;
using System;

public class GoalPlannerSubscriber : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _toggleVisualAction;
    public PoseWithCovarianceStampedMsg goalPath { get; private set; }
    private ROSConnection ros;
    private string goalPathTopic = "/move_base/GlobalPlanner/plan";
    private List<Vector3> trajectoryPoints = new List<Vector3>();

    public GameObject specklePrefab; 
    public float speckleSpacing = 0.5f;
    private List<GameObject> speckles = new List<GameObject>();
    private bool _isVisible = true;


    void Start()
    {
        if (specklePrefab == null)
        {
            Debug.LogError("Speckle Prefab not assigned.");
            return;
        }
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PathMsg>(goalPathTopic, UpdateGoalPath);
        _toggleVisualAction.action.performed += OnToggleVisualAction;
    }

    private void OnToggleVisualAction(InputAction.CallbackContext context)
    {
        _isVisible = !_isVisible;
    }

    private void UpdateGoalPath(PathMsg msg)
    {
        if (!_isVisible)
        {
            DestroySpeckles();
            return;
        }
        trajectoryPoints = ExtractTrajectoryPoints(msg);
        DrawSpeckledPath();
    }

    private void DestroySpeckles()
    {
        GameObject[] speckles = GameObject.FindGameObjectsWithTag("Speckle");
        foreach (GameObject speckle in speckles)
        {
            Destroy(speckle);
        }
    }

    private List<Vector3> ExtractTrajectoryPoints(PathMsg pathMsg)
    {
        List<Vector3> points = new List<Vector3>();
        foreach (var pose in pathMsg.poses)
        {
            float x = (float)pose.pose.position.y;
            float y = (float)pose.pose.position.z;
            float z = -(float)pose.pose.position.x;
            points.Add(new Vector3(x, y, z));
        }
        return points;
    }

    private void DrawSpeckledPath()
    {
        DestroySpeckles();
        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            Vector3 point = trajectoryPoints[i];
            CreateSpeckleAtPoint(point);

            if (i > 0 && Vector3.Distance(trajectoryPoints[i - 1], point) > speckleSpacing)
            {
                PlaceSpecklesAlongPath(trajectoryPoints[i - 1], point);
            }
        }
    }


    private void CreateSpeckleAtPoint(Vector3 point)
    {
        GameObject speckle = Instantiate(specklePrefab, point, Quaternion.identity);
        speckle.transform.parent = transform;
        speckle.tag = "Speckle"; 
        speckles.Add(speckle);
    }

    private void PlaceSpecklesAlongPath(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int numSpeckles = Mathf.CeilToInt(distance / speckleSpacing);
        Vector3 direction = (end - start).normalized;

        for (int i = 1; i <= numSpeckles; i++)
        {
            Vector3 position = start + direction * i * speckleSpacing;
            CreateSpeckleAtPoint(position);
        }
    }

    private void OnDestroy()
    {
        _toggleVisualAction.action.performed -= OnToggleVisualAction;
    }
}
