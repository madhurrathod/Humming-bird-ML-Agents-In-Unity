using UnityEngine;
using Unity.MLAgents;
/// <summary>
/// A humming bird Machine Learning Agent
/// </summary>
public class Hummingbird : Agent
{
    [Tooltip("Force you apply when moving")]
    public float moveForce = 2f;

    [Tooltip("Speed to pitch up or down")]
    public float pitchSpeed = 10f;

    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 10f;

    [Tooltip("Transform at the tip of the beak")]
    public Transform beakTip;

    [Tooltip("the agent's camera")]
    public Camera agentCamera;

    [Tooltip("whether this is training mode or gameplay mode")]
    public bool trainingMode;
    
    // The rigidbody of the Agent
    private new Rigidbody rigidbody;

    // The flower area that the agent is in
    private FlowerArea flowerArea;

    // The nearest flower to the agent
    private Flower nearestFlower;

    // Allows for smoother pitch changes
    private float smoothPitchChange = 0f;

    // Allows for smoother yaw changes
    private float smoothYawChange = 0f;

    // maximum angle that the bird can pitch up or down
    private const float MaxPitchAngle = 80f;

    // maximum distance from the beakTip to accept nectar collision
    private const float BeakTipRadius = 0.008f;

    // whether the agent is frozen (intentionally not flying)
    private bool frozen = false;

    /// <summary>
    /// The amount of nectar the agent has obtained this episode
    /// </summary>
    public float NectarObtained {get; private set;}

    
}
