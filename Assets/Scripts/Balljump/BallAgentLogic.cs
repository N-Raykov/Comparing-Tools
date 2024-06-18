using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BallAgentLogic : Agent
{
    Rigidbody rBody;
    [SerializeField] Transform target;
    [SerializeField] float speed;
    [SerializeField] float episodeLength;
    private float lastResetTime;
    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(-9, 0.5f, 0);
        lastResetTime = Time.time;

        target.localPosition = new Vector3(12 + Random.value * 8, Random.value * 3, Random.value * 10 - 5);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rBody.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.DiscreteActions[0];

        if (actions.DiscreteActions[1] == 2)
        {
            controlSignal.z = 1;
        }
        else
        {
            controlSignal.z = -actions.DiscreteActions[1];
        }


        //only gets input before reaching the end of the ramp
        if (transform.localPosition.x < 8.5)
        {
            rBody.AddForce(controlSignal * speed);
        }

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);

        if (distanceToTarget < 1.5f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        //ends episode if it falls off
        if(transform.localPosition.y < -1)
        {
            AddReward(-0.1f);
            EndEpisode();
        }

        if(Time.time >= lastResetTime + episodeLength)
        {
            lastResetTime = Time.time;
            AddReward(-0.1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = (int)Input.GetAxisRaw("Vertical");
        discreteActionsOut[1] = (int)Input.GetAxisRaw("Horizontal");
    }
}
