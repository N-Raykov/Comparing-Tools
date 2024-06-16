using UnityEngine;

public class Fruit : MonoBehaviour
{
    [SerializeField] int tier;
    private FruitManager fruitManager;
    private ScoreManager scoreManager;
    private bool isMerging = false;
    void Start()
    {
        fruitManager = FindObjectOfType<FruitManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
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

                int scoreIncrease = (int)Mathf.Pow(2, tier + 1);
                scoreManager.AddScore(scoreIncrease);
            }
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("GroundedFruit"))
        {
            // Tag the fruit as grounded when it touches the ground or another grounded fruit
            gameObject.tag = "GroundedFruit";
        }
    }
}