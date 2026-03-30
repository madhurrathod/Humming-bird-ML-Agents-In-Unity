using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a collectioon of flower plants and attatched flowers
/// </summary>
public class FlowerArea : MonoBehaviour
{
    // The diameter of the area where the agent and flowers can be used for observing relative distance from agent to flower
    public const float AreaDiameter = 20f;

    // The list of all flower plants in this flower area (flower plants have multiple flowers)
    private List<GameObject> flowerPlants;

    // A lookup dictionary for looking up a flower from a nectar collider
    private Dictionary<Collider, Flower> nectarFlowerDictionary;

    /// <summary>
    /// The list of all flowers in the flower area
    /// </summary>
    public List<Flower> Flowers {get; private set;}

    /// <summary>
    /// Reset the flowers and flower plants
    /// </summary>
    public void ResetFlower()
    {
        // Rotate each flower plant around the Y axis and subtly around X and Z
        foreach(GameObject flowerPlant in flowerPlants)
        {
            float xRotation = UnityEngine.Random.Range(-5f,5f);
            float yRotation = UnityEngine.Random.Range(-180f,180f);
            float zRotation = UnityEngine.Random.Range(-5f,5f);
            flowerPlant.transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        }

        // Reset each flower
        foreach(Flower flower in Flowers)
        {
            flower.ResetFlower();
        }
    }

    /// <summary>
    /// Gets the <see cref="Flower"/> that a nectar collider belongs to
    /// </summary>
    /// <param name="collider"> The nectar collider </param>
    /// <returns> The matching flower </returns>
    public Flower GetFlowerFromNectar(Collider collider)
    {
        return nectarFlowerDictionary[collider];
    }

    /// <summary>
    /// Called when the area wakes up
    /// </summary>
    private void Awake()
    {
        // Initialize variables
        flowerPlants = new List<GameObject>();
        nectarFlowerDictionary = new Dictionary<Collider, Flower>();
        Flowers = new List<Flower>();
    }

    
}
