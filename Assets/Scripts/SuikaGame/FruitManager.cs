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
        int tier = Random.Range(0, Mathf.Max(1, currentMaxTier - 1));
        nextFruit = Instantiate(fruitPrefabs[tier], nextFruitPosition.position, Quaternion.identity);
        nextFruit.GetComponent<Rigidbody2D>().simulated = false;
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
        newFruit.tag = "GroundedFruit";
        if (tier > currentMaxTier)
        {
            currentMaxTier = tier;
        }
    }

    public void ResetFruits()
    {
        currentMaxTier = 1;
        Destroy(nextFruit);
        GenerateNextFruit();
        foreach (var fruit in GameObject.FindGameObjectsWithTag("GroundedFruit"))
        {
            Destroy(fruit);
        }
        foreach (var fruit in GameObject.FindGameObjectsWithTag("Fruit"))
        {
            Destroy(fruit);
        }
    }

    public GameObject GetNextFruit()
    {
        return nextFruit;
    }
}

