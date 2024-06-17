using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Player : Agent
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float leftBoundary = 0f;
    [SerializeField] float rightBoundary = 10f;
    [SerializeField] float dropCooldown = 1f;

    private FruitManager fruitManager;
    private ScoreManager scoreManager;
    private GameObject currentFruit;
    private bool holdingFruit = false;
    private float lastDropTime = 0f;
    private bool canDrop = true;

    private Vector3 startingPos;

    void Start()
    {
        fruitManager = FindObjectOfType<FruitManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        startingPos = transform.localPosition;
    }

    void Update()
    {
        CheckGameOver();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startingPos;
        scoreManager.ResetScore();
        fruitManager.ResetFruits();
        canDrop = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(currentFruit ? currentFruit.transform.localPosition : Vector3.zero);

        GameObject nextFruit = fruitManager.GetNextFruit();
        sensor.AddObservation(nextFruit ? nextFruit.transform.localPosition : Vector3.zero);

        foreach (var groundedFruit in GameObject.FindGameObjectsWithTag("GroundedFruit"))
        {
            sensor.AddObservation(groundedFruit.transform.localPosition);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        canDrop = true;

        Move(actions.DiscreteActions[0]);
        if (actions.DiscreteActions[1] == 1 && canDrop && holdingFruit && Time.time >= lastDropTime + dropCooldown)
        {
            DropFruit();
            canDrop = false;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = (int)Input.GetAxisRaw("Horizontal");
        discreteActionsOut[1] = HandleInput();
    }

    void Move(int direction)
    {
        float move = direction * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x + move, leftBoundary, rightBoundary), transform.position.y, transform.position.z);
    }

    int HandleInput()
    {
        if (Input.GetKey(KeyCode.Space) && holdingFruit && Time.time >= lastDropTime + dropCooldown)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    void DropFruit()
    {
        currentFruit.transform.parent = null;
        currentFruit.GetComponent<Rigidbody2D>().simulated = true;
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
                    AddReward(-1.0f); // Penalty for losing the game
                    EndEpisode();
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
