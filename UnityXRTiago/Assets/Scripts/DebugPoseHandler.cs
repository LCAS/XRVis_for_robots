// using UnityEngine;

// public class DebugPoseHandler : MonoBehaviour
// {
//     public PlayerPositionPublisher playerPositionPublisher;
//     public RobotPoseSubscriber robotPoseSubscriber;

//     void Update()
//     {
//         if (playerPositionPublisher == null || robotPoseSubscriber == null)
//         {
//             Debug.LogError("PlayerPositionPublisher or TiagoROSPositionSub is not set.");
//             return;
//         }

//         // Access the player's ROS pose
//         var playerPose = playerPositionPublisher.rosPlayerPose;
//         if (playerPose != null)
//         {
//             Debug.Log("Player Pose: " + playerPose.pose.position + " - " + playerPose.pose.orientation);

//         }

//         // Access the robot's ROS pose
//         var unityRobotPose = robotPoseSubscriber.unityRobotPose;
//         var rosRobotPose = robotPoseSubscriber.rosRobotPose;

//         if (rosRobotPose != null)
//         {
//             Debug.Log("Tiago Pose: " + rosRobotPose.pose.pose.position + " - " + rosRobotPose.pose.pose.orientation);
//         }

//         if (unityRobotPose != null)
//         {
//             Debug.Log("Tiago Pose: " + unityRobotPose.localPosition + " - " + unityRobotPose.localRotation);
//         }
//     }
// }
