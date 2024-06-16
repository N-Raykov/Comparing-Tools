using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float leftBoundary = 0f;
    [SerializeField] float rightBoundary = 10f;
    [SerializeField] float dropCooldown = 1f;

    private FruitManager fruitManager;
    private GameObject currentFruit;
    private bool holdingFruit = false;
    private float lastDropTime = 0f;

    void Start()
    {
        fruitManager = FindObjectOfType<FruitManager>();
    }

    void Update()
    {
        Move();
        HandleInput();
        CheckGameOver();
    }

    void Move()
    {
        float move = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x + move, leftBoundary, rightBoundary), transform.position.y, transform.position.z);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && holdingFruit && Time.time >= lastDropTime + dropCooldown)
        {
            DropFruit();
        }
    }

    void DropFruit()
    {
        currentFruit.transform.parent = null;
        currentFruit.GetComponent<Rigidbody2D>().simulated = true; // Simulate the fruit when dropped
        holdingFruit = false;
        lastDropTime = Time.time;
        fruitManager.HoldNextFruit();
    }

    void CheckGameOver()
    {
        if (holdingFruit)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(currentFruit.transform.position, 0.5f);
            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject.CompareTag("GroundedFruit"))
                {
                    // Game over logic here
                    Debug.Log("Game Over!");
                }
            }
        }
    }

    public void HoldFruit(GameObject fruit)
    {
        currentFruit = fruit;
        holdingFruit = true;
        fruit.transform.parent = transform;
        fruit.GetComponent<Rigidbody2D>().simulated = false; // Ensure the fruit is not simulated when held
    }
}