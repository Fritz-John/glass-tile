

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFollowPlayer : MonoBehaviour
{
    private Transform playerCamera;

    public void SetPlayerCamera(Transform camera)
    {
        playerCamera = camera;
    }

    private void LateUpdate()
    {
        if (playerCamera != null)
        {
            transform.LookAt(transform.position + playerCamera.rotation * Vector3.forward, playerCamera.rotation * Vector3.up);
            
        }
    }
}

