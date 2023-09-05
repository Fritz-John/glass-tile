using Microsoft.Win32.SafeHandles;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnotherController : NetworkBehaviour
{
    Vector3 PlayerMovementInput;
    Vector2 PlayerMouseInput;
    float xRot;

    [SerializeField] Transform PlayerCamera;
    [SerializeField] Rigidbody rb;
    [SerializeField] float Speed;
    [SerializeField] float slideSpeed;
    [SerializeField] float Sensitivity;
    [SerializeField] float JumpForce;
    [SerializeField] LayerMask pushableLayer;

    private GameObject pushableObject;

    private Ray cameraRay;
    private RaycastHit cameraHit;

    private Ray pushRay;
    private RaycastHit pushHit;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (!isLocalPlayer)
        {
            PlayerCamera.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        PlayerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        PlayerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        MovePlayer();
        MovePlayerCamera();
        PlayerPushDetect();
    }
    void MovePlayer()
    {
        Vector3 MoveVector = transform.TransformDirection(PlayerMovementInput) * Speed;
        rb.velocity = new Vector3(MoveVector.x,rb.velocity.y,MoveVector.z);

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.9f);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }
    void MovePlayerCamera()
    {
        xRot -= PlayerMouseInput.y * Sensitivity;
        xRot = Mathf.Clamp(xRot, -55f, 55f);
        transform.Rotate(0f, PlayerMouseInput.x * Sensitivity, 0f);
        PlayerCamera.transform.localRotation = Quaternion.Euler(xRot,0,0);
    }
    void PlayerPushDetect()
    {
        cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        //Debug.DrawRay(cameraRay.origin, cameraRay.direction * 1f, Color.red);

        if (Physics.Raycast(cameraRay, out cameraHit, 1.5f))
        {

            if (cameraHit.collider.name == "Activator")
            {
                if (Input.GetKeyDown(KeyCode.E) && !TileSpawner.instance.isActivated)
                {
                    //CmdSpawnTiles();
                }
            }
            else if (cameraHit.collider.name == "Resetor")
            {
                if (Input.GetKeyDown(KeyCode.E) && TileSpawner.instance.isActivated)
                {
                    //CmdResetTiles();
                }
            }

        }
        if (Physics.Raycast(cameraRay, out pushHit, 2f, pushableLayer))
        {

            pushableObject = pushHit.collider.gameObject;

            if (Input.GetKeyDown(KeyCode.Q) && isLocalPlayer)
            {
                CmdPushPlayer(pushableObject);

            }
            if (Input.GetKey(KeyCode.V) && isLocalPlayer)
            {
                //CmdPullPlayer(pushableObject);

            }
            Debug.Log(pushableObject);
        }
        else
        {

            pushableObject = null;
        }
    }

    [Command]
    public void CmdPushPlayer(GameObject obj)
    {
        if (obj != null)
        {
            RpcPushPlayer(obj);
        }
    }

    [ClientRpc]
    private void RpcPushPlayer(GameObject objToPush)
    {
        Rigidbody rb = objToPush.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 slidingDirection = transform.forward;

            // Set the desired velocity magnitude for pushing
            float pushSpeed = 10.0f; // Adjust this value as needed

            // Calculate the desired velocity based on slidingDirection and pushSpeed
            Vector3 desiredVelocity = slidingDirection.normalized * pushSpeed;

            // Calculate the velocity change needed to achieve the desired velocity
            Vector3 velocityChange = desiredVelocity - rb.velocity;

            // Apply a force to the Rigidbody to achieve the velocity change
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}

