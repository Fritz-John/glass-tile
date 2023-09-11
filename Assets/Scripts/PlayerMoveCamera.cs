using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerMoveCamera : NetworkBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxVelocityChange = 10.0f;

    [Header("Camera Transform")]
    public Transform cameraTransform;

    [Header("Camera Movement")]
    public float lookSensitivity = 2f;
    public float smoothTime = 0.1f;

    [Header("Push Strenght")]
    public float slideSpeed = 10f;

    [Header("Custom Gravity")]
    public float gravity = -9.81f;
    public bool AllowAirControl = true;

    [Header("Prefab Breaked Tiles")]
    public GameObject breakableTiles;

   
    private Rigidbody rb;

    private Vector2 PlayerMouseInput;
    private float xRot;
    private bool isGrounded;
    private float yDistance = 0;
    private GameObject[] respawnPoint;
    private Vector3 currentVelocity;
    private Vector3 playerCurrentVelocity;
    private Ray cameraRay;
    private RaycastHit hit;
    private RaycastHit cameraHit;
    private RaycastHit pushHit;
    
    private GameObject pushableObject;

    private float xRotVelocity = 0f;
    private float horizontalInput;
    private float verticalInput;
    private GameObject tilesBroken;
  
    private Dictionary<GameObject, bool> disabledObjects = new Dictionary<GameObject, bool>();

    private List<int> usedSpawnPoints = new List<int>();
    [SerializeField] LayerMask pushableLayer;
 
    
 
    

    bool isNotMoving = false;


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

        if (!PlayerNameChange.isRenaming)
        {


            cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            PlayerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            float sphereCastRadius = 0.3f;

            isGrounded = Physics.SphereCast(transform.position, sphereCastRadius, Vector3.down, out hit, 0.9f);

            MyInput();
            PushableObject();
            BreakableObject();
            MovePlayerCamera();
            RespawnPoint();
            ActivatorReset();
       


            Debug.DrawRay(cameraRay.origin, cameraRay.direction * 3f, Color.red);


            if (Input.GetKeyDown(KeyCode.P))
            {
                Cursor.lockState = CursorLockMode.Locked;
                isNotMoving = false;
            }


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                isNotMoving = true;
            }

        }
    }

    private void FixedUpdate()
    {
        if (!isOwned)
        {
            return;
        }
        if (!PlayerNameChange.isRenaming)
        {

            Move();
        }
        else
        {
            rb.velocity = Vector3.zero;
        }

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


    //private void PushableObject()
    //{
    //    Vector3 halfExtents = new Vector3(0.5f, 0.5f, 1.5f);
    //    RaycastHit hit;

    //    if (Physics.BoxCast(transform.position, halfExtents, transform.forward, out hit, transform.rotation, 3f, pushableLayer))
    //    {
    //        pushableObject = hit.collider.gameObject;
    //        if (Input.GetButtonDown("Fire1"))
    //        {

    //            CmdPushPlayer(pushableObject);
    //        }
    //    }
    //}
    private void PushableObject()
    {
        if (Physics.Raycast(cameraRay, out pushHit, 3f, pushableLayer))
        {
            pushableObject = pushHit.collider.gameObject;
            if (Input.GetButtonDown("Fire1"))
            {
                Rigidbody rb = pushableObject.GetComponent<Rigidbody>();

                if (rb != null)               
                    CmdPushPlayer(pushableObject);                  
      
            }
        }
    }
   
    private void RespawnPoint()
    {
        if (usedSpawnPoints.Count == respawnPoint.Length)
        {
            // Reset the usedSpawnPoints list if you want to reuse them all
            usedSpawnPoints.Clear();
        }

        for (int i = 0; i < respawnPoint.Length; i++)
        {
            yDistance = Mathf.Abs(transform.position.y - respawnPoint[i].transform.position.y);
        }

        if (yDistance >= 10)
        {
            int random;
            do
            {
                random = Random.Range(0, respawnPoint.Length); // Generate a random index
            }
            while (usedSpawnPoints.Contains(random)); // Keep generating until an unused point is found
            usedSpawnPoints.Add(random);
            transform.position = respawnPoint[random].transform.position;
            rb.velocity = Vector3.zero;
        }
    }
    
    private void MyInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // when to jump
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            

            Jump();

        }
    }
    private void Move()
    {
      
       if(isGrounded || AllowAirControl)
        {
            Vector3 cameraForward = cameraTransform.forward;

            cameraForward.y = 0;

            cameraForward.Normalize();


            Vector3 targetVelocity = (cameraForward * verticalInput + cameraTransform.right * horizontalInput) * moveSpeed;


            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        

        rb.AddForce(new Vector3(0, -gravity * rb.mass, 0));


    }

    private void Jump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
    private void PushBackward(GameObject rb)
    {


        Rigidbody rigidBody = rb.GetComponent<Rigidbody>();

        Vector3 slidingDirection = transform.forward;

        Vector3 targetVelocity = slidingDirection * slideSpeed;

        rigidBody.AddForce(targetVelocity, ForceMode.VelocityChange);

    }

    [Command(requiresAuthority = true)]
    public void CmdPushPlayer(GameObject rb)
    {
        RpcPushPlayer(rb);
    }

    [ClientRpc]
    private void RpcPushPlayer(GameObject rb)
    {

        PushBackward(rb);
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
        if (objectToDisable != null && !disabledObjects.ContainsKey(objectToDisable))
        {
            disabledObjects[objectToDisable] = true;

            objectToDisable.GetComponent<BoxCollider>().enabled = false;
            objectToDisable.GetComponent<MeshRenderer>().enabled = false;

            tilesBroken = Instantiate(breakableTiles, objectToDisable.transform.position, objectToDisable.transform.rotation);
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
    #region OldMove
    //private void Move()
    //{
    //    float horizontalInput = Input.GetAxis("Horizontal");
    //    float verticalInput = Input.GetAxis("Vertical");

    //    Vector3 forwardDirection = cameraTransform.forward;
    //    forwardDirection.y = 0f;
    //    forwardDirection.Normalize();
    //    Vector3 rightDirection = cameraTransform.right;
    //    rightDirection.y = 0f;
    //    rightDirection.Normalize();
    //    Vector3 movementDirection = (forwardDirection * verticalInput + rightDirection * horizontalInput).normalized;

    //    Vector3 desiredVelocity = new Vector3(movementDirection.x * moveSpeed, rb.velocity.y, movementDirection.z * moveSpeed);

    //    playerCurrentVelocity = rb.velocity;

    //    Vector3 customGravity = new Vector3(0f, gravity, 0f);


    //    rb.AddForce(customGravity, ForceMode.Acceleration);


    //    Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    //    Vector3 verticalVelocity = new Vector3(0f, rb.velocity.y, 0f);


    //    float dampingFactor = 0.6f; 
    //    horizontalVelocity *= dampingFactor;


    //    rb.velocity = horizontalVelocity + verticalVelocity;

    //    // Debug.Log(movementDirection);
    //    if (movementDirection != Vector3.zero)
    //    {

    //        float forceMagnitude = 1.0f; 
    //        Vector3 velocityChange = (desiredVelocity - rb.velocity) * forceMagnitude;
    //        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    //    }
    //}
    #endregion
}


