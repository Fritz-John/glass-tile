using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

// gives access to MoreMountain's Scripts
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

public class PlayerMoveCamera : NetworkBehaviour
{
    public Camera playerCamera;
    CustomNM manager;

    [Header("Player Health")]
    [SyncVar]
    public int playerLife = 3;
    public int setplayerLife;
    public Transform heartContainer; 
    public Transform heartContainerPreview;
    public GameObject heartPrefab;


    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxVelocityChange = 10.0f;

    private float setMovespeed;

    [Header("Camera Transform")]
    public Transform cameraTransform;

    [Header("Camera Movement")]
    public float lookSensitivity = 2f;
    public float smoothTime = 0.1f;

    [Header("Push Strenght")]
    public float slideSpeed = 10f;
    public float rangeDetect = 1f;
    public float cooldownDuration = 1.0f;
    public Image cooldownImage;

    [Header("Custom Gravity")]
    public float gravity = -9.81f;
    public bool AllowAirControl = true;

    [Header("Feedbacks Player")]
    [Tooltip("AUDIO > Sfx should be added here to make the Sfx work")]
    [SerializeField] private MMF_Player jumpSFX;
    [SerializeField] private MMF_Player landSFX;

    [Header("Layer")]
    [SerializeField] LayerMask pushableLayer;

    [SerializeField] Button disconnectBtn;
    
    private Rigidbody playerRb;
    private bool isCooldown = false;       
    private float cooldownTimer = 0.0f;
    private Vector2 PlayerMouseInput;
    private float xRot;
    private bool isGrounded;

    private float yDistance = 0;
    private float yDistanceDeath = 0;

    private GameObject[] respawnPoint;
    private GameObject[] deathRespawnPoint;
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
   

    private List<int> usedSpawnPoints = new List<int>();


    PlayerNameChange playerNameChange;
    TimerScript timerScript;

    private Vector3 predictedPushVelocity = Vector3.zero;
    public AudioListener audioListener;

    public bool stunned = false;
    private bool hasLanded = false;
    void Start()
    {

        playerRb = GetComponent<Rigidbody>();
        setMovespeed = moveSpeed;
        timerScript = FindObjectOfType<TimerScript>();

        //Cursor.lockState = CursorLockMode.Locked;
        respawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint");
        deathRespawnPoint = GameObject.FindGameObjectsWithTag("DeathSpawn");
        setplayerLife = playerLife;
        playerNameChange = GetComponent<PlayerNameChange>();
        if (!isLocalPlayer)
        {
            cameraTransform.gameObject.SetActive(false);
            audioListener.enabled = false;
        }
        if (isLocalPlayer)
        {
            audioListener.enabled = true;
            CmdAssignPlayerAuthority(GetComponent<NetworkIdentity>());
            TextFollowPlayer[] textFollower = FindObjectsOfType<TextFollowPlayer>();
            foreach (TextFollowPlayer t in textFollower)
            {
                if (t != null)
                {
                    t.SetPlayerCamera(playerCamera.transform);
                }
            }
                
        }

        manager = GameObject.Find("NETWORK MANAGER").GetComponent<CustomNM>();
        disconnectBtn.onClick.AddListener(manager.Disconnect);

        CmdSetPlayerHealth(playerLife);
        
        // To initialize the Sfx
        jumpSFX = GameObject.Find("Jump SFX").GetComponent<MMF_Player>();
        jumpSFX.Initialization();
        landSFX = GameObject.Find("Landing SFX").GetComponent<MMF_Player>();
        landSFX.Initialization();
    }

    void Update()
    {
        if (!isLocalPlayer)
        {     
            return;
        }

        if (playerLife <= 1)
        {
            DeathRespawnPoint();
        }
        else
        {
            RespawnPoint();
        }    

        float sphereCastRadius = 0.1f;
        isGrounded = Physics.SphereCast(transform.position, sphereCastRadius, Vector3.down, out hit, 0.9f);


        if (!playerNameChange.isRenaming)
        {
            cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            PlayerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            if (!stunned)
            {
                MyInput();
                Jump();
            }
          
            PushableObject();        
            MovePlayerCamera();
            
            ActivatorReset();
      

            Debug.DrawRay(cameraRay.origin, cameraRay.direction * rangeDetect, Color.red);


            if (Input.GetKeyDown(KeyCode.P))
            {
                Cursor.lockState = CursorLockMode.Locked;
                
            }


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
             
            }
          
            //Debug.Log(playerLife);
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

    public void PlayerHit()
    {
        if (playerLife > 0 && TileSpawner.instance.isActivated)
        {
            playerLife--;
            CmdSetPlayerHealth(playerLife);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetPlayerHealth(int newHealth)
    {  
        playerLife = newHealth;

        

        RpcSetPlayerLife(newHealth);
    }
    [ClientRpc]
    void RpcSetPlayerLife(int newHealth)
    {

        UpdateUI(newHealth);
    }

    void UpdateUI(int newHealth)
    {
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform childs in heartContainerPreview)
        {
            Destroy(childs.gameObject);

        }

        for (int i = 0; i < newHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 100, 0);
        }

        if (isServer)
        {
            for (int i = 0; i < newHealth; i++)
            {
                GameObject hearts = Instantiate(heartPrefab, heartContainerPreview);
                hearts.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 100, 0);

                
                NetworkServer.Spawn(hearts);
            }
        }
        else
        {

            for (int i = 0; i < newHealth; i++)
            {
                GameObject hearts = Instantiate(heartPrefab, heartContainerPreview);
                hearts.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 100, 0);
            }
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

                    timerScript.CmdStartCountdown();
                }
            }   

            else if (cameraHit.collider.name == "Resetor" && !TileSpawner.instance.isDestroyingTiles)
            {

                if (Input.GetKeyDown(KeyCode.E) && TileSpawner.instance.isActivated)
                {
                    CmdResetTiles();
                    timerScript.CmdStopCountdown();
                }
            }
            else if (cameraHit.collider.name == "TpSpawn" && !TileSpawner.instance.isDestroyingTiles && playerLife > 0)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SpawnNow();
                }
            }
         
          

        }
    }
    private void PushableObject()
    {
        if (Physics.Raycast(cameraRay, out pushHit, rangeDetect, pushableLayer))
        {
            pushableObject = pushHit.collider.gameObject;
           
            if (Input.GetButtonDown("Fire1"))
            {
                if (!isCooldown)
                {
                    Rigidbody rb = pushableObject.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        Vector3 slidingDirection = transform.forward;
                        predictedPushVelocity = slidingDirection * slideSpeed;
                        CmdPushPlayer(pushableObject, predictedPushVelocity);
                    }
                      

                    isCooldown = true;
                    cooldownTimer = cooldownDuration;
                   
                }

            }
           
        }
        if (isCooldown)
        {
            
            cooldownTimer -= Time.deltaTime;
           

            if (cooldownTimer <= 0)
            {
                cooldownImage.fillAmount = 0;
                isCooldown = false;
            }
            cooldownImage.fillAmount = cooldownTimer / cooldownDuration;
        }
    }

    private void SpawnNow()
    {
        int random;
        do
        {
            random = Random.Range(0, respawnPoint.Length);
        }
        while (usedSpawnPoints.Contains(random));
        usedSpawnPoints.Add(random);
        transform.position = respawnPoint[random].transform.position;
        playerRb.velocity = Vector3.zero;
        stunned = false;
    }
   
    private void RespawnPoint()
    {
        if (usedSpawnPoints.Count == respawnPoint.Length)
        {
       
            usedSpawnPoints.Clear();
        }

        for (int i = 0; i < respawnPoint.Length; i++)
        {
            yDistance = Mathf.Abs(transform.position.y - respawnPoint[i].transform.position.y);
        }

        if (yDistance >= 15)
        {
            SpawnNow();
            PlayerHit();
         
        }
    }
    private void DeathRespawnPoint()
    {
        for (int i = 0; i < deathRespawnPoint.Length; i++)
        {
            yDistanceDeath = Mathf.Abs(transform.position.y - deathRespawnPoint[i].transform.position.y);
        }

        if (yDistanceDeath >= 10)
        {
            int random;

            random = Random.Range(0, deathRespawnPoint.Length);

            transform.position = deathRespawnPoint[random].transform.position;     
            if (playerLife > 0)
            {
                PlayerHit();
            }

            stunned = false;
            playerRb.velocity = Vector3.zero;
        }
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");


    }
    private void Move()
    {   
       if(isGrounded || AllowAirControl && !playerNameChange.isRenaming)
        {
            Vector3 cameraForward = cameraTransform.forward;

            cameraForward.y = 0;

            cameraForward.Normalize();


            Vector3 targetVelocity = (cameraForward * verticalInput + cameraTransform.right * horizontalInput) * setMovespeed;


            Vector3 velocity = playerRb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            playerRb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
       

        if (playerNameChange.isRenaming && isGrounded)
        {
            setMovespeed = 0;                
        }
        else
        {
            setMovespeed = moveSpeed;         
        }



        playerRb.AddForce(new Vector3(0, -gravity * playerRb.mass, 0));

    }
    private void Jump()
    {  
        if (isGrounded && !hit.collider.CompareTag("Breakable"))
        {
            if (!hasLanded)
            {           
                landSFX.PlayFeedbacks();
                hasLanded = true;
            }
        }
        else
        {
            hasLanded = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !hit.collider.CompareTag("Breakable"))
        {
            jumpSFX.PlayFeedbacks();
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            /*  Di ko alam kung saan toh dapat... Basta ito yung code para mag play yung landing sound
             *  landSFX.PlayFeedbacks();
             */
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
    private void PushBackward(GameObject rb, Vector3 predictedPushVelocity)
    {

        Rigidbody rigidBody = rb.GetComponent<Rigidbody>();

        rigidBody.AddForce(predictedPushVelocity, ForceMode.VelocityChange);

   
    }

    [Command(requiresAuthority = true)]
    public void CmdPushPlayer(GameObject rb, Vector3 predictedPushVelocity)
    {
        RpcPushPlayer(rb, predictedPushVelocity);
    }

    [ClientRpc]
    private void RpcPushPlayer(GameObject rb, Vector3 predictedPushVelocity)
    {

        PushBackward(rb, predictedPushVelocity);
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
        //TileSpawner.instance.SpawnLights();
    }

    [Command]
    public void CmdResetTiles()
    {
        TileSpawner.instance.ResetTiles();
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
    #region Old push
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
    #endregion
}


