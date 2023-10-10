using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

// To access MoreMountains' Scripts
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

public class TileManager : NetworkBehaviour
{
   
    [Header("Prefab Breaked Tiles")]
    public GameObject breakableTiles;

    private GameObject tilesBroken;

    private Dictionary<GameObject, bool> disabledObjects = new Dictionary<GameObject, bool>();

    [Header("SFX for DestroyTiles")]
    public AudioSource breakGlass;
    public AudioClip breakglassClip;
    public AudioClip breakglassClipShort;
    private AudioClip breakingGlassHolder;

    private TimerScript timerScript;
    
    [Header("Gun SFX")]
    [Tooltip("AUDIO > Sfx should be added here to make the Sfx work")]
    private MMF_Player gunRifleSfx;
    
    // Use this to play the gun sfx
    // gunRifleSfx.PlayFeedbacks();
    
    void Start()
    {
        timerScript = FindObjectOfType<TimerScript>();
        
                
        gunRifleSfx = GameObject.Find("Gun Rifle SFX").GetComponent<MMF_Player>();
        gunRifleSfx.Initialization();
    }

  
    void Update()
    {
        if(timerScript.timer == 0)
        {
            breakingGlassHolder = breakglassClipShort;
        }
        else
        {
            breakingGlassHolder = breakglassClip;
        }
    }
   
    private void OnCollisionEnter(Collision other)
    {
        GameObject player = other.gameObject;
        if (player.CompareTag("Player") && this.tag == "Breakable")
        {
            PlayerMoveCamera playerMoveCamera = player.GetComponent<PlayerMoveCamera>();
            playerMoveCamera.stunned = true;
           
            CmdDisableObject(gameObject);

            StartCoroutine(RotatePlayer(player));
            
        }
    }
    private IEnumerator RotatePlayer(GameObject player)
    {
        Quaternion startRotation = player.transform.rotation;


        Vector3 cameraForward = Camera.main.transform.forward; 
        Vector3 lookDirection = cameraForward;
        lookDirection.y = 0; 
        lookDirection.Normalize(); 
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        targetRotation = Quaternion.Euler(-90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);

        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            player.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.transform.rotation = targetRotation;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    GameObject player = other.gameObject;
    //    if (other.CompareTag("Player"))
    //    {
    //        CmdDisableObject(gameObject);


    //        //CmdDestroy(gameObject);

    //    }
    //}

    [ClientRpc]
    public void RpcPlaySounds()
    {
        breakGlass.clip = breakingGlassHolder;
        breakGlass.Play();

    }
    [Command(requiresAuthority = false)]
    public void CmdPlayBreakSound()
    {
        
        RpcPlaySounds();
    }

    private IEnumerator DestroyObjectWithDelay(GameObject objectToDestroy)
    {
        yield return new WaitForSeconds(5f);

        if (objectToDestroy != null)
        {
            NetworkServer.Destroy(objectToDestroy);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdDisableObject(GameObject objectToDestroy)
    {
        RpcDisableObject(objectToDestroy);
        StartCoroutine(DestroyObjectWithDelay(objectToDestroy));
        CmdPlayBreakSound();

        TileSpawner.instance.RemoveTile(objectToDestroy);

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

    //[ClientRpc]
    //public void RpcPlaySounds()
    //{
    //    breakGlass.clip = breakglassClip;
    //    breakGlass.Play();
    //}




}
