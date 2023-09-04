using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.Pong;


public class PlayerMoveCamera : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    public float lookSensitivity = 2f;
    public float cameraSmoothing = 5f;

    private Rigidbody rb;
    public Transform cameraTransform;




    private bool isGrounded;

    private float rotationX = 0f;
    float yDistance = 0;
    public float slideSpeed = 10f;
    //public Transform respawnPoint;

    GameObject[] respawnPoint;
    private Vector3 currentVelocity;

    private Vector3 currentCameraPosition;
    private Quaternion currentCameraRotation;

    private Ray cameraRay;
    private RaycastHit cameraHit;

    private Ray pushRay;
    private RaycastHit pushHit;
    public LayerMask pushableLayer;
    private GameObject pushableObject;

    void Start()
    {


        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        respawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint");

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

        for (int i = 0; i < respawnPoint.Length; i++)
        {
            yDistance = Mathf.Abs(transform.position.y - respawnPoint[i].transform.position.y);
        }


        //float yDistance = Mathf.Abs(transform.position.y - respawnPoint.transform.position.y);
     

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.9f);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.9f))
            {
                if (hit.collider.CompareTag("Breakable"))
                {
                    CmdPlayBreakSound();
                    CmdDisableObject(hit.collider.gameObject);
                    // CmdDestroyObject(hit.collider.gameObject);

                }
            }
        }
        cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(cameraRay, out cameraHit, 1f))
        {
            //Debug.Log(cameraHit.collider.name);
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
        if (Physics.Raycast(cameraRay, out pushHit, 1.5f, pushableLayer))
        {

            pushableObject = pushHit.collider.gameObject;

            if (Input.GetKeyDown(KeyCode.Q) && isLocalPlayer)
            {
                CmdPushPlayer(pushableObject);
                //pushableObject.GetComponent<Rigidbody>().AddForce(transform.forward * 10f, ForceMode.Impulse);
            }
            if (Input.GetKey(KeyCode.V) && isLocalPlayer)
            {
                CmdPullPlayer(pushableObject);
                //pushableObject.GetComponent<Rigidbody>().AddForce(transform.forward * 10f, ForceMode.Impulse);
            }
            Debug.Log(pushableObject);
        }
        else
        {

            pushableObject = null;
        }




        //    Vector3 playerMiddle = transform.position + Vector3.up * 0.1f;
        //pushRay = new Ray(playerMiddle, transform.forward);

        //Debug.DrawRay(playerMiddle, transform.forward * 1f, Color.red);



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
            int random = Random.Range(0, respawnPoint.Length);
            transform.position = respawnPoint[random].transform.position;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void FixedUpdate()
    {

        if (!isLocalPlayer)
        {
            return;
        }
        Move();
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
        currentVelocity = rb.velocity;

        

            Vector3 velocityChange = (desiredVelocity - rb.velocity);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        
       

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
            Vector3 relativeForce = slidingDirection * slideSpeed - rb.velocity;

            rb.AddForce(relativeForce, ForceMode.Impulse);
        }
    }

    [Command]
    public void CmdPullPlayer(GameObject obj)
    {
        if (obj != null)
        {
            RpcPullPlayer(obj);
        }
    }

    [ClientRpc]
    private void RpcPullPlayer(GameObject objToPush)
    {
        Rigidbody rb = objToPush.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 slidingDirection = -transform.forward;
            Vector3 relativeForce = slidingDirection * 6f - rb.velocity;

            rb.AddForce(relativeForce, ForceMode.Impulse);
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
        if (objectToDisable != null)
        {
            objectToDisable.GetComponent<BoxCollider>().enabled = false;
            objectToDisable.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}