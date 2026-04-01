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
    
}
