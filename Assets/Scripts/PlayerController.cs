using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f; 

    public float lookSensitivity = 2f;
    public float cameraSmoothing = 5f; 

    private Rigidbody rb;
    public Transform cameraTransform;

    private bool isGrounded;

    private float rotationX = 0f;

    //public Transform respawnPoint;

    GameObject respawnPoint;
    private Vector3 currentCameraPosition;
    private Quaternion currentCameraRotation;

    private Ray cameraRay; 
    private RaycastHit cameraHit;
    
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
        cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(cameraRay, out cameraHit, 1f))
        {
            Debug.Log(cameraHit.collider.name);
            if (cameraHit.collider.name == "Activator")
            {
                if(Input.GetKeyDown(KeyCode.E) && !TileSpawner.instance.isActivated)
                {
                    TileSpawner.instance.SpawnTiles();
                }
            }
            else if (cameraHit.collider.name == "Resetor")
            {
                if (Input.GetKeyDown(KeyCode.E) && TileSpawner.instance.isActivated)
                {
                    TileSpawner.instance.ResetTiles();
                }
            }
            
        }


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

        if (yDistance >= 10)
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