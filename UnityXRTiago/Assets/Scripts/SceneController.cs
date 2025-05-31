using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

[RequireComponent(typeof(ARPlaneManager))] 

public class SceneController : MonoBehaviour {
    [SerializeField]
    private InputActionReference _togglePlanesAction;
    
    [SerializeField]
    private InputActionReference _leftActivateAction;
    
    [SerializeField]
    private InputActionReference _rightActivateAction;

    [SerializeField]
    private XRRayInteractor _leftRayInteractor;

    [SerializeField]
    private GameObject _grabbableCube;

    [SerializeField]
    private GameObject _prefab;

    private ARPlaneManager _planeManager; 
    private ARAnchorManager _anchorManager; 

    public Transform referenceTransform;
    private List<ARAnchor> _anchors = new();

    private bool _isVisible = false;
    private int _numPlanesAddedOccured = 0;
    private ROSConnection ros;
    private string poseStampedTopic = "/raycast_pose";

    void Start() {
        _planeManager = GetComponent<ARPlaneManager>(); // gets reference for the ARPlaneManager

        if (_planeManager is null) {
            Debug.LogError("-> Can't find 'ARPlaneManager' :( ");
        }

        _anchorManager = GetComponent<ARAnchorManager>();
        if (_anchorManager is null)
        {
            Debug.LogError("-> Can't find 'ARAnchorManager' :(");
        }

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(poseStampedTopic);

        _togglePlanesAction.action.performed += OnTogglePlanesAction;
        _planeManager.planesChanged += OnPlanesChanged;
        _anchorManager.anchorsChanged += OnAnchorsChanged;
        _leftActivateAction.action.performed += OnLeftActivateAction;
        _rightActivateAction.action.performed += OnRightActivateAction;
    }

    private void OnAnchorsChanged(ARAnchorsChangedEventArgs obj)
    {
        foreach (var removedAnchor in obj.removed)
        {
            _anchors.Remove(removedAnchor);
            Destroy(removedAnchor.gameObject);
        }
    }

    private void OnRightActivateAction(InputAction.CallbackContext obj) {
        // SetTiagoGoalPose();
        Debug.Log("-> Right Activate Action not set");
    }

    private void OnLeftActivateAction(InputAction.CallbackContext obj) {
        SetTiagoGoalPose();
    }

    private void SetTiagoGoalPose()
    {
        if (_leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (_anchors.Count > 0)
            {
                ARAnchor previousAnchor = _anchors[_anchors.Count - 1]; // Get the last anchor
                _anchors.RemoveAt(_anchors.Count - 1); // Remove the last anchor from the list
                Destroy(previousAnchor.gameObject); // Destroy the previous anchor's GameObject
            }

            Vector3 hitPosition = hit.point;
            Quaternion hitRotation = Quaternion.LookRotation(hit.normal, Vector3.up);
            GameObject instance = Instantiate(_prefab, hitPosition, hitRotation);

            if (instance.GetComponent<ARAnchor>() == null)
            {
                ARAnchor anchor = instance.AddComponent<ARAnchor>();

                if (anchor != null)
                {
                    _anchors.Add(anchor);
                }
                else
                {
                    Debug.LogError("-> CreateAnchoredObject() - anchor is null!");
                }
            }

            Vector3 relativePosition = referenceTransform.InverseTransformPoint(hitPosition);
            Quaternion relativeRotation = Quaternion.Inverse(referenceTransform.rotation) * hitRotation;

            PoseStampedMsg poseStampedMsg = new PoseStampedMsg
            {
                header = new HeaderMsg
                {
                    stamp = new TimeMsg(),
                    frame_id = "map"
                },
                pose = new PoseMsg
                {
                    position = new PointMsg(
                        -relativePosition.z,
                        relativePosition.x,
                        relativePosition.y
                    ),
                    
                    orientation = new QuaternionMsg(
                        relativeRotation.x,
                        relativeRotation.z,
                        relativeRotation.w,
                        relativeRotation.y
                    )
                }
            };
            ros.Publish(poseStampedTopic, poseStampedMsg);
        }
        else
        {
            Debug.LogFormat("-> No hit detected!");
        }
    }

    private void OnTogglePlanesAction(InputAction.CallbackContext obj) {
        _isVisible = !_isVisible;
        float fillAlpha = _isVisible ? 0.0f : 0.1f;
        float lineAlpha = _isVisible ? 0.0f : 1.0f;
        foreach (var plane in _planeManager.trackables) {
            SetPlaneAlpha(plane, fillAlpha, lineAlpha);
        }
    }

    private void SetPlaneAlpha(ARPlane plane, float fillAlpha, float lineAlpha) {
        var meshRenderer = plane.GetComponentInChildren<MeshRenderer>();
        var lineRenderer = plane.GetComponentInChildren<LineRenderer>();

        if (meshRenderer != null) {
            Color color = meshRenderer.material.color;
            color.a = fillAlpha;
            meshRenderer.material.color = color;
        }

        if (lineRenderer != null) {
            // Get the current start and end colors
            Color startColor = lineRenderer.startColor;
            Color endColor = lineRenderer.endColor;

            // Set the alpha component
            startColor.a = lineAlpha;
            endColor.a = lineAlpha;

            // Apply the new colors with updated alpha
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args) {
        if (args.added.Count > 0) {
            _numPlanesAddedOccured++;

        }
    }

    private void OnDestroy() {
        _togglePlanesAction.action.performed -= OnTogglePlanesAction;
        _planeManager.planesChanged -= OnPlanesChanged;
        _anchorManager.anchorsChanged -= OnAnchorsChanged;
        _leftActivateAction.action.performed -= OnLeftActivateAction;
        _rightActivateAction.action.performed -= OnRightActivateAction;

    }
}
