# Mixed Reality Visualisations for Interpretable Transparent Robot Behaviour

This repository contains code and data used in preparation of the "Mixed Reality Visualisations for Interpretable Transparent Robot Behaviour" by Omar Ali published to the 26th Annual TAROS CONFERENCE in 2025.

The project enables real-time Mixed Reality/Extended Reality visualidation of robot behaviour to support Human-Robot Interaction (HRI). A Meta Quest 3 XR headset is used to visualise the TIAGo robot's pose and planned trajectory in the user's environment using Unity and ROS integration.
It consists of the different scripts used for the XR device to grab the robot's pose and goal path trajectories. As well the relevant scripts to visualise them with pre-fab gameobjects.

![image](https://github.com/user-attachments/assets/aa449ae7-bd66-4749-8088-a1e69c323e12)


## System Architecture

![image](https://github.com/user-attachments/assets/74f389c1-a1f5-4f8b-82ea-e3f7873a9705)

The system consists of:

* Meta Quest 3 XR headset running a Unity application
* TIAGo robot running ROS Noetic
* A host machine running the ROS Master and handling communication

Unity communicates with ROS using the [ROS-TCP-Connector](https://github.com/Unity-Technologies/ROS-TCP-Connector) package.

## Unity Setup

Unity version: `2022.3.41`

### Required Packages

* `ROS-TCP-Connector (v0.7.0-preview)`
* `AR Foundation`
* `OpenXR Plugin`

Add ROS-TCP-Connector via:

```
https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector
```

### Custom Scripts

Attached to the `XR Origin` in Unity:
* `RobotPoseSubscriber.cs`: Subscribes to `/amcl_pose` and updates the robot’s visual position in Unity.
* `PlayerPositionPublisher.cs`: Publishes headset position to `/unity_user_pose`.
* `GoalPlannerSubscriber.cs`: Subscribes to `/move_base/GlobalPlanner/plan` and visualizes the planned trajectory.
* `DebugPanel.cs`: Displays status and pose information in the headset.
* 
![image](https://github.com/user-attachments/assets/7cef7d5e-118e-4580-ac54-a3e6c214d538)

Additionally a custom "Speckle" game object was used to act at the visualiser for the path. 

![image](https://github.com/user-attachments/assets/a01cc7dd-5dec-48d4-9aae-025a69b71410)

## ROS Setup

Version: ROS Noetic

### Required Packages

* `ros_tcp_endpoint` (https://github.com/Unity-Technologies/ROS-TCP-Endpoint)
* `move_base`
* `amcl`

### ROS Topics

* `/amcl_pose` -> Unity (robot pose)
* `/move_base/GlobalPlanner/plan` -> Unity (trajectory)
* `/unity_user_pose` <- Unity (headset pose)

## Networking

1. Identify the IP of the ROS Master.
2. Choose an open TCP port (e.g., `11312`).
3. Start the ROS TCP Endpoint:

   ```
   roslaunch ros_tcp_endpoint endpoint.launch tcp_ip:=<ROS_MASTER_IP> tcp_port:=<PORT>
   ```
4. In Unity, go to `Robotics -> ROS Settings` and set the IP and port.
![image](https://github.com/user-attachments/assets/16e0d892-fa84-499f-9756-02956f4a62fe)


## Calibration

1. Place the robot at the origin of the ROS map.
2. The user stands directly over the robot with the headset on.
3. Press the Quest "Home" button to reset the headset origin.
4. Confirm the visual axes align with the robot’s position.
![image](https://github.com/user-attachments/assets/0f705d6a-7f13-4b5d-b9ad-7c164dac0f62)

---

This work was supported by the Engineering and Physical Sciences Research Council
and AgriFoRwArdS CDT [EP/S023917/1].

---
