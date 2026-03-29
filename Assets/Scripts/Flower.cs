using UnityEngine;

///<summary>
/// Manages single flower with nectar
/// </summary>
public class Flower : MonoBehaviour
{
    [Tooltip("The color when the flower is full")]
    public Color fullFlowerColor = new Color(1f,0f,.3f);

    [Tooltip("The color when the flower is empty")]
    public Color emptyFlowerColor = new Color(.5f, 0f, 1f);

    /// <summary>
    /// The trigger collider representing the nectar
    /// </summary>
    [HideInInspector]
    public Collider nectarCollider;

    // The solid collider representing flower petal
    private Collider flowerCollider;

    // The flower's material
    private Material flowerMaterial;
    
    /// <summary>
    /// A Vector pointing straight out of the flower
    /// </summary>
    public Vector3 FlowerUpVector
    {
        get
        {
            return nectarCollider.transform.up;
        }
    }

    /// <summary>
    /// The center position of the nectar collider
    /// </summary>
    public Vector3 FlowerCenterPosition
    {
        get
        {
            return nectarCollider.transform.position;
        }
    }

    // The amount of nectar remaining in the flower
    public float NectarAmount {get; private set;}

    /// <summary>
    /// Whether the flower has any nectar remaining
    /// </summary>
    public bool HasNectar
    {
        get
        {
            return NectarAmount > 0f;
        }
    }

    /// <summary>
    /// Attempts to remove nectar from the flower
    /// </summary>
    /// <param name="amount"> The amount of nectar to remove </param>
    /// <returns> The actual amount successfully removed </returns>
    public float Feed(float amount)
    {
        // Track how much nectar was successfully taken(cannot take more than is available)
        float nectarTaken = Mathf.Clamp(amount, 0f, NectarAmount);

        // subtract the nectar
        NectarAmount -= amount;
        
        if(NectarAmount <= 0f)
        {
            // No nectar remaining
            NectarAmount = 0f;

            // Disable the flower and nectar colliders
            flowerCollider.gameObject.SetActive(false);
            nectarCollider.gameObject.SetActive(false);

            // Change the flower color to indicate that it is empty
            flowerMaterial.SetColor("_BaseColor", emptyFlowerColor);
        }

        // Return the amount of nectar that was taken
        return NectarAmount;
    }

    /// <summary>
    /// Refill the flower
    /// </summary>
    public void ResetFlower()
    {
        // Refill the nectar
        NectarAmount = 1f;

        // Enable the flower and nectar colliders
        flowerCollider.gameObject.SetActive(true);
        nectarCollider.gameObject.SetActive(true);

        // Change the flower color to indicate that it is full
        flowerMaterial.SetColor("_BaseColor", fullFlowerColor);
    }

    /// <summary>
    /// Called when the flower wakes up
    /// </summary>
    private void Awake()
    {
        // Find the flower's mesh renderer and get the main materials
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        flowerMaterial = meshRenderer.material;

        // Find flower and nectar colliders
        flowerCollider = transform.Find("FlowerCollider").GetComponent<Collider>();
        nectarCollider = transform.Find("FlowerNectarCollider").GetComponent<Collider>();
    }
}
