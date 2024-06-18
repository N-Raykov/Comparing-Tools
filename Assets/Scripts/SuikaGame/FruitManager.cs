using UnityEngine;

public class FruitManager : MonoBehaviour
{
    public GameObject[] fruitPrefabs;
    [SerializeField] Transform heldFruitPosition;
    [SerializeField] Transform nextFruitPosition;
    private int currentMaxTier = 1;
    private GameObject nextFruit;
    private Player player;

    void Start()
    {
        player = transform.parent.GetComponentInChildren<Player>();
        GenerateNextFruit();
        HoldNextFruit();
    }

    public void GenerateNextFruit()
    {
        int tier = Random.Range(0, Mathf.Max(1, currentMaxTier - 1));
        nextFruit = Instantiate(fruitPrefabs[tier], nextFruitPosition.position, Quaternion.identity, transform);
        nextFruit.GetComponent<Rigidbody2D>().simulated = false;
        InitializeFruit(nextFruit);
    }

    public void HoldNextFruit()
    {
        if (nextFruit != null)
        {
            nextFruit.transform.position = heldFruitPosition.position;
            player.HoldFruit(nextFruit);
        }
        GenerateNextFruit();
    }

    public void SpawnFruit(int tier, Vector3 position)
    {
        GameObject newFruit = Instantiate(fruitPrefabs[tier], position, Quaternion.identity, transform);
        newFruit.tag = "GroundedFruit";
        InitializeFruit(newFruit);
        if (tier > currentMaxTier)
        {
            currentMaxTier = tier;
        }
    }

    private void InitializeFruit(GameObject fruit)
    {
        Fruit fruitScript = fruit.GetComponent<Fruit>();
        if (fruitScript != null)
        {
            fruitScript.Initialize(this, transform.parent.GetComponentInChildren<ScoreManager>(), player);
        }
    }

    public void ResetFruits()
    {
        currentMaxTier = 1;
        Destroy(nextFruit);
        GenerateNextFruit();
        foreach (Transform fruit in transform)
        {
            if (fruit.CompareTag("GroundedFruit") || fruit.CompareTag("Fruit"))
            {
                Destroy(fruit.gameObject);
            }
        }
    }

    public GameObject GetNextFruit()
    {
        return nextFruit;
    }

    public int GetCurrentMaxTier()
    {
        int maxTier = 0;
        foreach (Transform fruit in transform)
        {
            Fruit fruitScript = fruit.GetComponent<Fruit>();
            if (fruitScript != null && fruitScript.GetTier() > maxTier)
            {
                maxTier = fruitScript.GetTier();
            }
        }
        return maxTier;
    }
}
