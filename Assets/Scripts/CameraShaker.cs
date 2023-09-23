using System.Collections;
using UnityEngine;
using Mirror;

public class CameraShaker : NetworkBehaviour
{
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.2f;

    [SyncVar]
    private bool isShaking = false;

    private float shakeTimer = 0f;
    private GameObject currentPlayerInZone;

    private void OnTriggerStay(Collider other)
    {
        GameObject player = other.gameObject;

        if (player.CompareTag("Player"))
        {    
            isShaking = true;
            CmdStartShake(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject player = other.gameObject;

        if (player.CompareTag("Player"))
        {
            CmdStopShake();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdStartShake(GameObject player)
    {
        RpcShakeCamera(player);
    }

    [Command(requiresAuthority = false)]
    private void CmdStopShake()
    {
        RpcStopShake();
    }
    [ClientRpc]
    private void RpcStopShake()
    {
        isShaking = false;
    }
    [ClientRpc]
    private void RpcShakeCamera(GameObject player)
    {
        PlayerMoveCamera playerCameras = player.GetComponent<PlayerMoveCamera>();
        Camera playerCam = playerCameras.playerCamera;
        StartCoroutine(Shake(playerCam));
    }


    private IEnumerator Shake(Camera camera)
    {
        while (isShaking)
        {
            Vector3 originalPosition = camera.transform.localPosition;
            Vector3 randomOffset = Random.insideUnitSphere * shakeMagnitude;

            camera.transform.localPosition = originalPosition + randomOffset;

            shakeTimer -= Time.deltaTime;

            yield return null;
        }

        camera.transform.localPosition = Vector3.zero;
    }
}
