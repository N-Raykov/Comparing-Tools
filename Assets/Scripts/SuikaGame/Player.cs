using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class Player : Agent
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float leftBoundary = 0f;
    [SerializeField] float rightBoundary = 10f;
    [SerializeField] float dropCooldown = 1f;
    [SerializeField] float maxIdleTime = 5f; // Max time allowed without dropping a fruit
    [SerializeField] Transform cornerTarget; // Target transform to check for rewards
    [SerializeField] LayerMask fruitLayer; // Layer mask to detect fruits

    private FruitManager fruitManager;
    private ScoreManager scoreManager;
    private GameObject currentFruit;
    private bool holdingFruit = false;
    private float lastDropTime = 0f;
    private bool canDrop = true;

    private Vector3 startingPos;

    void Start()
    {
        fruitManager = transform.parent.GetComponentInChildren<FruitManager>();
        scoreManager = transform.parent.GetComponentInChildren<ScoreManager>();
        startingPos = transform.localPosition;
    }

    void Update()
    {
        CheckGameOver();

        // Punish if the agent doesn't drop a fruit after maxIdleTime seconds
        if (holdingFruit && Time.time >= lastDropTime + maxIdleTime)
        {
            SetReward(-5.0f);
            lastDropTime = Time.time;
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startingPos;
        scoreManager.ResetScore();
        fruitManager.ResetFruits();
        Destroy(currentFruit);
        fruitManager.HoldNextFruit();
        canDrop = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the player's position and idle time
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(Time.time - (lastDropTime + maxIdleTime));

        // Add the current fruit's position and tier
        if (currentFruit != null)
        {
            Fruit currentFruitScript = currentFruit.GetComponent<Fruit>();
            sensor.AddObservation(currentFruit.transform.localPosition);
            sensor.AddObservation(currentFruitScript != null ? currentFruitScript.GetTier() : -1);

            // Perform a raycast downward from the current fruit to detect the fruit below
            RaycastHit2D hit = Physics2D.Raycast(currentFruit.transform.position, Vector2.down, Mathf.Infinity, fruitLayer);
            if (hit.collider != null)
            {
                Fruit hitFruit = hit.collider.GetComponent<Fruit>();
                if (hitFruit != null)
                {
                    sensor.AddObservation(hitFruit.transform.localPosition);
                    sensor.AddObservation(hitFruit.GetTier());

                    // Add the positions and tiers of fruits colliding with the hit fruit
                    List<Fruit> collidingFruits = hitFruit.GetCollidingFruits();
                    foreach (Fruit collidingFruit in collidingFruits)
                    {
                        if (collidingFruit != null)
                        {
                            sensor.AddObservation(collidingFruit.transform.localPosition);
                            sensor.AddObservation(collidingFruit.GetTier());
                        }
                    }
                }
                else
                {
                    sensor.AddObservation(Vector3.zero);
                    sensor.AddObservation(-1); // Indicator for no fruit
                }
            }
            else
            {
                sensor.AddObservation(Vector3.zero);
                sensor.AddObservation(-1); // Indicator for no fruit
            }
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(-1); // Indicator for no fruit
        }

        // Add the next fruit's position and tier
        GameObject nextFruit = fruitManager.GetNextFruit();
        if (nextFruit != null)
        {
            Fruit nextFruitScript = nextFruit.GetComponent<Fruit>();
            sensor.AddObservation(nextFruit.transform.localPosition);
            sensor.AddObservation(nextFruitScript != null ? nextFruitScript.GetTier() : -1);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(-1); // Indicator for no fruit
        }

        // Add the positions and tiers of grounded fruits
        foreach (Transform groundedFruit in fruitManager.transform)
        {
            if (groundedFruit.CompareTag("GroundedFruit"))
            {
                Fruit groundedFruitScript = groundedFruit.GetComponent<Fruit>();
                sensor.AddObservation(groundedFruit.transform.localPosition);
                sensor.AddObservation(groundedFruitScript != null ? groundedFruitScript.GetTier() : -1);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        canDrop = true;
        if (actions.DiscreteActions[0] == 2)
        {
            Move(1);
        }
        else if (actions.DiscreteActions[0] == 1)
        {
            Move(0);
        }
        else
        {
            Move(-1);
        }

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
        float newPosX = transform.localPosition.x + move;
        float fruitRadius = currentFruit.GetComponent<CircleCollider2D>().radius * currentFruit.transform.localScale.x;

        // Clamp the new position within local boundaries
        newPosX = Mathf.Clamp(newPosX, leftBoundary + fruitRadius, rightBoundary - fruitRadius);

        transform.localPosition = new Vector3(newPosX, transform.localPosition.y, transform.localPosition.z);
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
        currentFruit.transform.parent = fruitManager.transform;  // Ensure the fruit is parented to the FruitManager
        currentFruit.GetComponent<Rigidbody2D>().simulated = true;
        currentFruit.tag = "Fruit";
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
                    SetReward(-20.0f);
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

    public void RewardForCornerMerge(Vector3 fruitPosition)
    {
        float distanceToCorner = Vector3.Distance(fruitPosition, cornerTarget.localPosition);
        if (distanceToCorner < 3.0f)
        {
            AddReward(Vector3.Distance(fruitPosition, cornerTarget.localPosition));
        }
    }
}
