using UnityEngine;
using System.Collections.Generic;

public class Fruit : MonoBehaviour
{
    [SerializeField] int tier;
    private FruitManager fruitManager;
    private ScoreManager scoreManager;
    private Player player; // Reference to the player for adding rewards
    private bool isMerging = false;
    private List<Fruit> collidingFruits = new List<Fruit>();

    public void Initialize(FruitManager fruitMgr, ScoreManager scoreMgr, Player plyr)
    {
        fruitManager = fruitMgr;
        scoreManager = scoreMgr;
        player = plyr;
    }

    public int GetTier()
    {
        return tier;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Fruit otherFruit = collision.gameObject.GetComponent<Fruit>();
        if (otherFruit != null)
        {
            collidingFruits.Add(otherFruit);
        }

        if (otherFruit != null && otherFruit.tier == tier && collision.gameObject.CompareTag("GroundedFruit"))
        {
            if (fruitManager != null && tier < fruitManager.fruitPrefabs.Length - 1 && !isMerging && !otherFruit.isMerging)
            {
                isMerging = true;
                otherFruit.isMerging = true;

                int newTier = tier + 1;
                Vector3 position = (transform.position + otherFruit.transform.position) / 2;
                Destroy(gameObject);
                Destroy(otherFruit.gameObject);
                fruitManager.SpawnFruit(newTier, position);

                // Reward for merging
                int scoreIncrease = scoreManager != null ? scoreManager.GetTierScore(tier) : 0;
                if (scoreManager != null) scoreManager.AddScore(tier);
                if (player != null)
                {
                    player.AddReward(scoreIncrease / 10);
                    if (newTier > fruitManager.GetCurrentMaxTier())
                    {
                        player.RewardForCornerMerge(position);
                    }
                }
            }

            if (otherFruit != null)
            {
                CheckForFruitChain();
            }
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("GroundedFruit"))
        {
            gameObject.tag = "GroundedFruit";
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Fruit otherFruit = collision.gameObject.GetComponent<Fruit>();
        if (otherFruit != null)
        {
            collidingFruits.Remove(otherFruit);
        }
    }

    public List<Fruit> GetCollidingFruits()
    {
        return collidingFruits;
    }

    private void CheckForFruitChain()
    {
        foreach (var fruit in collidingFruits)
        {
            if (fruit.GetTier() == this.tier + 1 || fruit.GetTier() == this.tier - 1)
            {
                if (player != null)
                {
                    player.AddReward(0.5f); // Reward for creating a fruit chain
                }
            }
        }
    }
}

