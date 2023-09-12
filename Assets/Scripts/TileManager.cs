using Mirror;
using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("Has been called");
        RpcPlaySounds();
    }

    [Command(requiresAuthority = false)]
    private void CmdDestroy(GameObject breaks)
    {
  
        RpcDestroy(breaks);
    }
    [ClientRpc]
    private void RpcDestroy(GameObject breaks)
    {
        NetworkServer.Destroy(breaks);
        tilesBroken = Instantiate(breakableTiles, breaks.transform.position, breaks.transform.rotation);
            
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
