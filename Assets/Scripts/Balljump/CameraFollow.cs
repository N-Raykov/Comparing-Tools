using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform ballAgentTransform;

    private Vector3 _cameraOffset;

    private void Start()
    {
        _cameraOffset = transform.position - ballAgentTransform.position;
    }

    private void LateUpdate()
    {
        transform.position = ballAgentTransform.position + _cameraOffset;
    }
}
