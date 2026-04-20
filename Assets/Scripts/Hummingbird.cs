using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
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
            flowerArea.ResetFlower();
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
            inFrontOfFlower = UnityEngine.Random.value > .5f;
        }
        // move the agent to a new random position 
        MoveToSafeRandomPosition(inFrontOfFlower);

        // Recalculate the neareset flower now that the agent has moved
        UpdateNearestFlower();
    }

    /// <summary>
    /// Called when an action is received from either the player input or the neural network
    /// 
    /// vectorAction[i] represents:
    /// Index 0: move vector x (+1 = right, -1 = left)
    /// Index 1: move vector y (+1 = up, -1 = down)
    /// Index 2: move vector z (+1 = forward, -1 = backward)
    /// Index 3: pitch angle (+1 = pitch up, -1 = pitch down)
    /// Index 4: yaw angle (+1 = turn right, -1 = turn left)
    /// </summary>
    /// <param name="actions">The action to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        float[] vectorAction = actions.ContinuousActions.Array;
        // Don't take actions if frozen
        if(frozen) return;

        // Calculate movement vector
        Vector3 move = new Vector3(vectorAction[0], vectorAction[1],vectorAction[2]);

        // Add force if the direction of move vector
        rigidbody.AddForce(move * moveForce);

        // Get the current rotation
        Vector3 rotationVector = transform.rotation.eulerAngles;

        // Calculate pitch and yaw rotation
        float pitchChange = vectorAction[3];
        float yawChange = vectorAction[4];

        // Calculate smooth rotation changes
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        // Calculate new pitch and yaw based on smoothed values
        // Clamp pitch to avoid flipping upside down
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;

        if(pitch > 180f) pitch -= 360;
        pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

        float yaw = rotationVector.y + smoothPitchChange * Time.fixedDeltaTime * yawSpeed;

        // Apply the new rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

    }

    private void MoveToSafeRandomPosition(bool inFrontOfFlower)
    {
        bool safePositionFound = false;
        int attemptsRemaining = 100; // prevent an infinite loop
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // Loop until a safe position is found or we run out of attempts
        while(!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining --;
            if (inFrontOfFlower)
            {
                // Pick a random flower
                Flower randomFlower = flowerArea.Flowers[UnityEngine.Random.Range(0,flowerArea.Flowers.Count)];

                // Position 10 to 20 cm in front of the flower
                float distanceFromFlower = UnityEngine.Random.Range(.1f,.2f);

                potentialPosition = randomFlower.transform.position + randomFlower.FlowerUpVector * distanceFromFlower;

                // Point beak at flower (bird's head is center of transform)
                Vector3 toFlower = randomFlower.FlowerCenterPosition - potentialPosition;

                potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);
            }
            else
            {
                // Pick a random height from the ground
                float height = UnityEngine.Random.Range(1.2f,2.5f);

                // Pick a random radius from the center of the area
                float radius = UnityEngine.Random.Range(2f,7f);

                // Pick a random direction rotated around the y axis
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180,180),0f);

                // combine height, radius and direction to pick a potential position
                potentialPosition = flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;

                // choose and set random starting pitch and yaw
                float pitch = UnityEngine.Random.Range(-60f,60f);
                float yaw = UnityEngine.Random.Range(-180f,180f);
                potentialRotation = Quaternion.Euler(pitch,yaw,0f);

                // check to see if the agent will collide with anything
                Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);

                // safe position has been found if no colliders are overlapped
                safePositionFound = colliders.Length == 0;
            }
            Debug.Assert(safePositionFound, "could not find a safe position to spawn");

            // set the position and rotation
            transform.position = potentialPosition;
            transform.rotation = potentialRotation;
        }
    }

    /// <summary>
    /// update the nearest flower to the agent
    /// </summary>
    private void UpdateNearestFlower()
    {
        foreach(Flower flower in flowerArea.Flowers)
        {
            if(nearestFlower == null && flower.HasNectar)
            {
                // No current nearest flower and this flower has nectar, so set to this flower
                nearestFlower = flower;
            }
            else if (flower.HasNectar)
            {
                // calculate distance to this flower and distance to the current nearest flower
                float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);
                float distanceToCurrentNearestFlower = Vector3.Distance(nearestFlower.transform.position, beakTip.position);

                // If current nearest flower is empty or this flower is closer, update the nearest flower
                if(!nearestFlower.HasNectar || distanceToFlower < distanceToCurrentNearestFlower)
                {
                    nearestFlower = flower;
                }
            }

        }
    }
}
