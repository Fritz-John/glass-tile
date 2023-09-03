using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float lookSensitivity = 2f;

    private Rigidbody rb;
    public Transform cameraTransform;

    private bool isGrounded;

    private float rotationX = 0f;
    float mouseX = 0f;
    float mouseY = 0f;
    //public Transform respawnPoint;

    private Quaternion targetCameraRotation;
    private Vector3 targetCameraPosition;
    GameObject respawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        respawnPoint = GameObject.Find("SpawnPoint");
        Application.targetFrameRate = 60;

        if (!isLocalPlayer)
        {
            cameraTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        float yDistance = Mathf.Abs(transform.position.y - respawnPoint.transform.position.y);
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

       
        Vector3 forwardDirection = cameraTransform.forward;
        forwardDirection.y = 0f;
        forwardDirection.Normalize();
        Vector3 rightDirection = cameraTransform.right;
        rightDirection.y = 0f;
        rightDirection.Normalize();
        Vector3 movementDirection = (forwardDirection * verticalInput + rightDirection * horizontalInput).normalized;

        rb.velocity = new Vector3(movementDirection.x * moveSpeed, rb.velocity.y, movementDirection.z * moveSpeed);

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.7f);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.7f))
            {
                if (hit.collider.CompareTag("Breakable"))
                {
                    CmdDestroyObject(hit.collider.gameObject);

                }
            }
        }
        // mouseX += Input.GetAxis("Mouse X") * lookSensitivity;
        // mouseY += Input.GetAxis("Mouse Y") * lookSensitivity;

        //mouseX = Mathf.Clamp(mouseX, -60f, 60f);

        //Quaternion target = Quaternion.Euler(-mouseX, mouseY, 0);
        //transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 3f);

        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -55f, 55f);

        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        if (Input.GetButtonDown("Fire1"))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(yDistance >= 10)
        {
            transform.position = respawnPoint.transform.position;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    [Command]
    void CmdDestroyObject(GameObject objectToDestroy)
    {
       
        if (objectToDestroy != null && objectToDestroy.CompareTag("Breakable"))
        {
            
            NetworkServer.Destroy(objectToDestroy);
        }
    }

}
