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


    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {   
        rigidbody = GetComponent<Rigidbody>();
        flowerArea = GetComponentInParent<FlowerArea>();

        // If not training mode, no max step, play forever
        if(!trainingMode) MaxStep = 0;
    }   

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            // only reset flowers in training when there is one agent per area
            flowerArea.ResetFlowers();
        }

        // Reset the nectar obtained
        NectarObtained = 0f;

        // zero out velocities so that movement stops before a new episode begins
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // Default to spawning in front of a flower
        bool inFrontOfFlower = true;
        if (trainingMode)
        {
            // spawn in front of flower 50% of the time during training
            inFrontOfFlower = UnityEnigne.Random.value > .5f;
        }
        // move the agent to a new random position 
        MoveToSafeRandomPosition(inFrontOfFlower);

        // Recalculate the neareset flower now that the agent has moved
        UpdateNearestFlower();
    }
}
