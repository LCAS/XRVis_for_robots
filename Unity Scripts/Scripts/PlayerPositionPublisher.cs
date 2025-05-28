using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class PlayerPositionPublisher : MonoBehaviour
{
    public Transform playerTransform;
    public Transform referenceTransform;
    public PoseStampedMsg rosPlayerPose { get; private set; }
    
    private string playerPoseTopic = "/unity_player_pose";
    private ROSConnection ros;

    void Start()
    {
        Debug.Log("-> starting PlayerPositionPublisher");
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>("/unity_player_pose");
    }

    void Update()
    {
        if (playerTransform == null || referenceTransform == null)
        {
            Debug.LogError("PlayerTransform or ReferenceTransform is not set.");
            return;
        }

        // Calculate the relative position and rotation
        Vector3 relativePosition = referenceTransform.InverseTransformPoint(playerTransform.position);
        Quaternion relativeRotation = Quaternion.Inverse(referenceTransform.rotation) * playerTransform.rotation;

        // Convert Unity coordinates to ROS coordinates
        Vector3 rosPosition = new Vector3(
            -relativePosition.z,
            relativePosition.x, 
            relativePosition.y
        );
        
        Quaternion rosRotation = new Quaternion(
            relativeRotation.x,
            relativeRotation.z,
            relativeRotation.w,
            relativeRotation.y
        );

        rosPlayerPose = new PoseStampedMsg
        {
            header = new HeaderMsg
            {
                stamp = new TimeMsg(),
                frame_id = "map"
            },
            pose = new PoseMsg
            {
                position = new PointMsg(rosPosition.x, rosPosition.y, rosPosition.z),
                orientation = new QuaternionMsg(rosRotation.x, rosRotation.y, rosRotation.z, rosRotation.w)
            }
        };

        ros.Publish(playerPoseTopic, rosPlayerPose);
    }
}
