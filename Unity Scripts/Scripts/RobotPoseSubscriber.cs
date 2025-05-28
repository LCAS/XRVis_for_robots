using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class RobotPoseSubscriber : MonoBehaviour
{
    public PoseWithCovarianceStampedMsg rosRobotPose { get; private set; }
    public Transform unityRobotPose;
    // public Transform rosMapOrigin;

    private ROSConnection ros;
    private string robotPoseTopic = "/robot_pose";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PoseWithCovarianceStampedMsg>(robotPoseTopic, UpdateRobotPose);
    }

    void UpdateRobotPose(PoseWithCovarianceStampedMsg poseMsg)
    {
        rosRobotPose = poseMsg;

        Vector3 unityPosition = new Vector3(
            (float)rosRobotPose.pose.pose.position.y,  // ROS Y -> Unity X
            (float)rosRobotPose.pose.pose.position.z,  // ROS Z -> Unity Y
            -(float)rosRobotPose.pose.pose.position.x  // ROS X -> Unity Z (negated)
        );

        Quaternion unityRotation = new Quaternion(
            (float)rosRobotPose.pose.pose.orientation.y,   // ROS Y -> Unity X
            (float)rosRobotPose.pose.pose.orientation.z,   // ROS Z -> Unity Y
            -(float)rosRobotPose.pose.pose.orientation.x,  // ROS X -> Unity Z (negated)
            -(float)rosRobotPose.pose.pose.orientation.w   // ROS W -> Unity W (negated)
        );

        unityRobotPose.localPosition = unityPosition;
        unityRobotPose.localRotation = unityRotation;
    }
}

