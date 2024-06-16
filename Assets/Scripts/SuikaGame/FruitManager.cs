using UnityEngine;

public class FruitManager : MonoBehaviour
{
    public GameObject[] fruitPrefabs;
    [SerializeField] Transform heldFruitPosition;
    [SerializeField] Transform nextFruitPosition;
    private int currentMaxTier = 1;
    private GameObject nextFruit;

    void Start()
    {
        GenerateNextFruit();
        HoldNextFruit();
    }

    public void GenerateNextFruit()
    {
        int tier = Random.Range(0, Mathf.Max(1, currentMaxTier - 1)); // Tier between 0 and 2 tiers below currentMaxTier
        nextFruit = Instantiate(fruitPrefabs[tier], nextFruitPosition.position, Quaternion.identity);
        nextFruit.GetComponent<Rigidbody2D>().simulated = false; // Ensure the fruit is not simulated
    }

    public void HoldNextFruit()
    {
        if (nextFruit != null)
        {
            nextFruit.transform.position = heldFruitPosition.position;
            FindObjectOfType<Player>().HoldFruit(nextFruit);
        }
        GenerateNextFruit();
    }

    public void SpawnFruit(int tier, Vector3 position)
    {
        GameObject newFruit = Instantiate(fruitPrefabs[tier], position, Quaternion.identity);
        newFruit.tag = "GroundedFruit"; // Tag the spawned fruit as grounded
        if (tier > currentMaxTier)
        {
            currentMaxTier = tier;
        }
    }
}
