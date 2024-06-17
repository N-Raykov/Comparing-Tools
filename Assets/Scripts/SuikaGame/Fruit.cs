// Fruit.cs
using UnityEngine;

public class Fruit : MonoBehaviour
{
    [SerializeField] int tier;
    private FruitManager fruitManager;
    private ScoreManager scoreManager;
    private Player player; // Reference to the player for adding rewards
    private bool isMerging = false;

    void Start()
    {
        fruitManager = FindObjectOfType<FruitManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        player = FindObjectOfType<Player>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Fruit otherFruit = collision.gameObject.GetComponent<Fruit>();
        if (otherFruit != null && otherFruit.tier == tier && collision.gameObject.CompareTag("GroundedFruit"))
        {
            if (tier < fruitManager.fruitPrefabs.Length - 1 && !isMerging && !otherFruit.isMerging)
            {
                isMerging = true;
                otherFruit.isMerging = true;

                int newTier = tier + 1;
                Vector3 position = (transform.position + otherFruit.transform.position) / 2;
                Destroy(gameObject);
                Destroy(otherFruit.gameObject);
                fruitManager.SpawnFruit(newTier, position);

                // Reward for merging
                int scoreIncrease = scoreManager.GetTierScore(tier);
                scoreManager.AddScore(tier);
                player.AddReward(scoreIncrease/25);
            }
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("GroundedFruit"))
        {
            gameObject.tag = "GroundedFruit";
        }
    }
}
