using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TileManager : NetworkBehaviour
{
    
    //public static PlaySoun instance;
    public GameObject breakaBle;


    [Header("Prefab Breaked Tiles")]
    public GameObject breakableTiles;

    private GameObject tilesBroken;

    private Dictionary<GameObject, bool> disabledObjects = new Dictionary<GameObject, bool>();

    public AudioSource breakGlass;
    public AudioClip breakglassClip;

    public float delay;


    void Start()
    {

    }

  
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject player = other.gameObject;
        if (other.CompareTag("Player"))
        {
            CmdDisableObject(gameObject);


            //CmdDestroy(gameObject);

        }
    }

    [ClientRpc]
    public void RpcPlaySounds()
    {
        breakGlass.clip = breakglassClip;
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
