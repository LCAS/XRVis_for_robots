using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;

public class DebugPanel : MonoBehaviour
{

    // public PlayerPositionPublisher playerPositionPublisher;
    // public RobotPoseSubscriber robotPoseSubscriber;
    private string player_x, player_y, player_z, player_qx, player_qy, player_qz, player_qw;
    private string tiago_x, tiago_y, tiago_z, tiago_qx, tiago_qy, tiago_qz, tiago_qw;
    private string player_loc, tiago_loc;
    
    private static Canvas   _canvas;
    private static Text     _debugText, _fpsText, _ipText, _tiagoPoseText, _playerPoseText,_statusText;
    private static ROSConnection ros;

    private float   _elapsedTime;
    private uint    _fpsSamples;
    private float   _sumFps;

    private Queue<string> _queuedMessages;

    private const int MAX_LINES = 23;

    private Transform _cameraTransform;
    private Vector3 _dirToPlayer = Vector3.zero;
    
    void Awake()
    {
        AcquireObjects();
        _elapsedTime = 0;
        _fpsSamples = 0;
        _fpsText.text = "0";
        _queuedMessages = new Queue<string>();

        Application.logMessageReceived += OnMessageReceived;
    }

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PoseWithCovarianceStampedMsg>("/robot_pose", UpdateRobotPose);
        ros.Subscribe<PoseStampedMsg>("/unity_player_pose", UpdatePlayerPose);
        ros.Subscribe<StringMsg>("/unity_action", UpdateTiagoAction);
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= OnMessageReceived;
    }

    private void AcquireObjects()
    {
        _canvas = this.gameObject.GetComponent<Canvas>();
        Transform ui = this.transform.Find("UI");
        
        _debugText = ui.Find("DebugText").GetComponent<Text>();
        _fpsText = ui.Find("FpsText").GetComponent<Text>();
        _statusText = ui.Find("StatusText").GetComponent<Text>();
        _ipText = ui.Find("IPText").GetComponent<Text>();
        _tiagoPoseText = ui.Find("TiagoPoseText").GetComponent<Text>();
        _playerPoseText = ui.Find("PlayerPoseText").GetComponent<Text>();
        _statusText.text = "Hi there, I'm Tiago!";
    }

    void OnMessageReceived(string message, string stackTrace, LogType type)
    {
        _queuedMessages.Enqueue(message);
    }
    
    // Update is called once per frame
    void Update()
    {
        _elapsedTime += Time.deltaTime;

        if(_elapsedTime > 0.5f)
        {
            //Update FPS every half second 
            _fpsText.text = ( Mathf.Round((_sumFps / _fpsSamples))).ToString();
            _ipText.text = ros.RosIPAddress + ":" + ros.RosPort.ToString();
            _elapsedTime = 0f;
            _sumFps = 0f;
            _fpsSamples = 0;
        }

        _sumFps += (1.0f / Time.smoothDeltaTime);
        _fpsSamples++;
                _dirToPlayer = (this.transform.position - _cameraTransform.position).normalized;
        _dirToPlayer.y = 0; // This ensures rotation only around the Y-axis
        this.transform.rotation = Quaternion.LookRotation( _dirToPlayer );
        
        if (_queuedMessages.Count > 0)
        {
            while (_queuedMessages.Count > 0)
            {
                _debugText.text += (_queuedMessages.Dequeue() + "\n");
            }
            TrimText();
        }  
    }

    private void UpdatePlayerPose(PoseStampedMsg playerPose)
    {
        player_x =  playerPose.pose.position.x.ToString("F2");
        player_y =  playerPose.pose.position.y.ToString("F2");
        player_z =  playerPose.pose.position.z.ToString("F2");
        player_qx = playerPose.pose.orientation.x.ToString("F2");
        player_qy = playerPose.pose.orientation.y.ToString("F2");
        player_qz = playerPose.pose.orientation.z.ToString("F2");
        player_qw = playerPose.pose.orientation.w.ToString("F2");
        player_loc = "Player: (" + player_x + ", " + player_y + ", " + player_z + ") - (" + player_qx + ", " + player_qy + ", " + player_qz + ", " + player_qw + ")";
        _playerPoseText.text = player_loc;
    }

    private void UpdateRobotPose(PoseWithCovarianceStampedMsg tiagoPose)
    {
        tiago_x =  tiagoPose.pose.pose.position.x.ToString("F2");
        tiago_y =  tiagoPose.pose.pose.position.y.ToString("F2");
        tiago_z =  tiagoPose.pose.pose.position.z.ToString("F2");
        tiago_qx = tiagoPose.pose.pose.orientation.x.ToString("F2");
        tiago_qy = tiagoPose.pose.pose.orientation.y.ToString("F2");
        tiago_qz = tiagoPose.pose.pose.orientation.z.ToString("F2");
        tiago_qw = tiagoPose.pose.pose.orientation.w.ToString("F2");
        tiago_loc = "Tiago: (" + tiago_x + ", " + tiago_y + ", " + tiago_z + ") - (" + tiago_qx + ", " + tiago_qy + ", " + tiago_qz + ", " + tiago_qw + ")";
        _tiagoPoseText.text = tiago_loc;
    }

    private void UpdateTiagoAction(StringMsg msg)
    {
        _statusText.text = msg.data;
    }

    public static void Clear()
    {
        if (_debugText is null) return;
        _debugText.text = "";
    }
    
    public static void Show()
    {
        SetVisibility(true);
    }

    public static void Hide()
    {
        SetVisibility(false);
    }
    
    public static void SetVisibility(bool visible)
    {
        if (_canvas is null) return;
        _canvas.enabled = visible;
    }
    
    public static void ToggleVisibility()
    {
        if (_canvas is null) return;
        _canvas.enabled = !_canvas.enabled;
    }
    
    public static void SetStatus(string message)
    {
        if (_statusText is null) return;
        _statusText.text = (message);
    }

    public static void SetIp(string message)
    {
        if (_ipText is null) return;
        _ipText.text = (message);
    }

    private static void TrimText()
    {
        string[] lines = _debugText.text.Split('\n');
        
        if (lines.Length > MAX_LINES)
        {
            _debugText.text = string.Join("\n", lines, lines.Length - MAX_LINES, MAX_LINES);
        }
    }
    
}