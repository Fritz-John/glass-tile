using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Unity.VisualScripting;

public class PlayerMoveCamera : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    public float lookSensitivity = 2f;
    public float cameraSmoothing = 5f;

    private Rigidbody rb;
    public Transform cameraTransform;

    Vector2 PlayerMouseInput;
    float xRot;

    private bool isGrounded;

    
    float yDistance = 0;
    public float slideSpeed = 10f;
    public float pullSpeed = 10f;
    public float gravity = -9.81f;
    //public Transform respawnPoint;

    GameObject[] respawnPoint;
    private Vector3 currentVelocity;
    private Vector3 playerCurrentVelocity;

    private Ray cameraRay;
    private RaycastHit hit;
    private RaycastHit cameraHit;
    private RaycastHit pushHit;

    public LayerMask pushableLayer;
    private GameObject pushableObject;

    public GameObject breakableTiles;
    public float smoothTime = 0.1f; // Adjust this value to control the smoothness

    private float xRotVelocity = 0f;
    public float jumpCameraSmoothing = 0.2f; // Adjust this value for camera smoothing during jumps

    private Vector3 cameraVelocity;


    void Start()
    {


        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        respawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint");

       

      

        if (!isLocalPlayer)
        {
            cameraTransform.gameObject.SetActive(false);
        }
        if (isLocalPlayer)
        {
            CmdAssignPlayerAuthority(GetComponent<NetworkIdentity>());
        }

    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        PlayerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        float sphereCastRadius = 0.2f;
        isGrounded = Physics.SphereCast(transform.position, sphereCastRadius, Vector3.down, out hit, 0.9f);

        MovePlayerCamera();
        Jump();  
        RespawnPoint();
        PushableObject();
        BreakableObject();
        ActivatorReset();


        //if (isGrounded && Input.GetButtonDown("Jump"))
        //{
        //    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        //}

     


        //Debug.DrawRay(cameraRay.origin, cameraRay.direction * 1f, Color.red);


        if (Input.GetButtonDown("Fire1"))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void FixedUpdate()
    {

        if (!isOwned)
        {
            return;
        }
        Move();
        

    }
    private void ActivatorReset()
    {
        if (Physics.Raycast(cameraRay, out cameraHit, 1.5f))
        {

            if (cameraHit.collider.name == "Activator")
            {
                if (Input.GetKeyDown(KeyCode.E) && !TileSpawner.instance.isActivated)
                {
                    CmdSpawnTiles();
                }
            }
            else if (cameraHit.collider.name == "Resetor")
            {
                if (Input.GetKeyDown(KeyCode.E) && TileSpawner.instance.isActivated)
                {
                    CmdResetTiles();
                }
            }

        }
    }
    private void BreakableObject()
    {
        if (isGrounded)
        {
            if (hit.collider.CompareTag("Breakable"))
            {
                CmdPlayBreakSound();
                CmdDisableObject(hit.collider.gameObject);
            }
        }

    }
    private void PushableObject()
    { 
        if (Physics.Raycast(cameraRay, out pushHit, 2f, pushableLayer))
        {
            pushableObject = pushHit.collider.gameObject;
            if (Input.GetKeyDown(KeyCode.Q))
            {
                CmdPushPlayer(pushableObject);
            }
           
        }

    }
    private void RespawnPoint()
    {
        for (int i = 0; i < respawnPoint.Length; i++)
        {
            yDistance = Mathf.Abs(transform.position.y - respawnPoint[i].transform.position.y);
        }

        if (yDistance >= 10)
        {
            int random = Random.Range(0, respawnPoint.Length);
            transform.position = respawnPoint[random].transform.position;
            rb.velocity = Vector3.zero;
        }
    }
    private void Jump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

    }
    private void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forwardDirection = cameraTransform.forward;
        forwardDirection.y = 0f;
        forwardDirection.Normalize();
        Vector3 rightDirection = cameraTransform.right;
        rightDirection.y = 0f;
        rightDirection.Normalize();
        Vector3 movementDirection = (forwardDirection * verticalInput + rightDirection * horizontalInput).normalized;

        Vector3 desiredVelocity = new Vector3(movementDirection.x * moveSpeed, rb.velocity.y, movementDirection.z * moveSpeed);

        playerCurrentVelocity = rb.velocity;

        Vector3 customGravity = new Vector3(0f, gravity, 0f);

        rb.AddForce(customGravity, ForceMode.Acceleration);


       // Debug.Log(movementDirection);
        if (movementDirection != Vector3.zero)
        {

            Vector3 velocityChange = (desiredVelocity - rb.velocity);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
      
    }
    void MovePlayerCamera()
    {
        xRot -= PlayerMouseInput.y * lookSensitivity;
        xRot = Mathf.Clamp(xRot, -55f, 55f);
        transform.Rotate(0f, PlayerMouseInput.x * lookSensitivity, 0f);

        float targetXRotation = xRot;
        float smoothXRotation = Mathf.SmoothDampAngle(cameraTransform.transform.localRotation.eulerAngles.x, targetXRotation, ref xRotVelocity, smoothTime);

        cameraTransform.transform.localRotation = Quaternion.Euler(smoothXRotation, 0, 0);
      
    }

    [Command]
    public void CmdAssignPlayerAuthority(NetworkIdentity playerObject)
    {
        if (playerObject != null)
        {
            playerObject.AssignClientAuthority(connectionToClient);
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
            float pushForce = slideSpeed;
            float maxPushSpeed = 10f; // Adjust this value as needed
            currentVelocity = rb.velocity;
            // Limit the maximum speed of the pushed object to avoid teleportation
            if (rb.velocity.magnitude < maxPushSpeed)
            {
                rb.AddForce(slidingDirection * pushForce + playerCurrentVelocity + currentVelocity, ForceMode.Impulse);
            }
        }
    }

    #region PullPlayer
    //[Command]
    //public void CmdPullPlayer(GameObject obj)
    //{
    //    if (obj != null)
    //    {
    //        RpcPullPlayer(obj);
    //    }
    //}

    //[ClientRpc]
    //private void RpcPullPlayer(GameObject objToPush)
    //{
    //    Rigidbody rb = objToPush.GetComponent<Rigidbody>();

    //    if (rb != null)
    //    {
    //        Vector3 slidingDirection = -transform.forward;
    //        Vector3 relativeForce = slidingDirection * pullSpeed;
    //        currentVelocity = rb.velocity;
    //        rb.AddForce(relativeForce + currentVelocity, ForceMode.VelocityChange);
    //    }
    //}
    #endregion


    [Command]
    public void CmdSpawnTiles()
    {
        TileSpawner.instance.SpawnTiles();
    }

    [Command]
    public void CmdResetTiles()
    {
        TileSpawner.instance.ResetTiles();
    }
    [Command]
    public void CmdPlayBreakSound()
    {
        PlaySoun.instance.RpcPlaySounds();
    }

    [Command]
    void CmdDisableObject(GameObject objectToDestroy)
    {
        RpcDisableObject(objectToDestroy);
        StartCoroutine(DestroyObjectWithDelay(objectToDestroy));
       
    }
    private IEnumerator DestroyObjectWithDelay(GameObject objectToDestroy)
    {
        yield return new WaitForSeconds(3f);

        if (objectToDestroy != null)
        {
            NetworkServer.Destroy(objectToDestroy);
        }
    }
    [ClientRpc]
    void RpcDisableObject(GameObject objectToDisable)
    {
        if (objectToDisable != null)
        {
            
            objectToDisable.GetComponent<BoxCollider>().enabled = false;
            objectToDisable.GetComponent<MeshRenderer>().enabled = false;
            GameObject tilesBroken = Instantiate(breakableTiles, objectToDisable.transform.position, objectToDisable.transform.rotation);
            StartCoroutine(DestroyBreakableTileWithDelay(tilesBroken));
        }
    }
    private IEnumerator DestroyBreakableTileWithDelay(GameObject objectToDestroy)
    {
        yield return new WaitForSeconds(3f);

        if (objectToDestroy != null)
        {
            NetworkServer.Destroy(objectToDestroy);
        }
    }
}