using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Telepathy;
using static UnityEngine.Rendering.DebugUI.Table;

public class TileSpawner : NetworkBehaviour
{
    [Header("Gameobjects Prefabs")]
    public GameObject breakablePrefab;
    public GameObject lightSphere;

    [Header("Spacing of Tiles")]
    public float spacing = 7f;
    public int rowCount = 10;
    public int colCount = 2;

    [Header("Lights On Bridge")]
    public Transform spawnerLight;
    public Transform spawnerLight2;

    [Header("Destroyed Tiles")]
    public float DestroyTilesSpeed = 1.0f;
    public GameObject tilesBrokenPrefab;

    [Header("Explode of glass breaking")]
    public float minexplosionForce;
    public float maxexplosionForce;

    [Header("Reference Scripts")]
    public TimerScript timerScript;
    public PlayerCounter playerCount;

    public static TileSpawner instance;

    [SyncVar]
    public bool isActivated = false;

    //public Animator activate;
    //public Animator reset;
    GameObject tile;

    List<GameObject> tiles = new List<GameObject>();
    
    public bool isDestroyingTiles = false;


    public bool startedGame = false;

 
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (isServer)
        {
            SpawnLights();
        }
      
    }
    private void Update()
    {
        //if(timer.timer <= 0  )
        //{
        //    if (!isDestroyingTiles)
        //    {
        //        isDestroyingTiles = true;

        //        StartCoroutine(DestroyTilesWithDelay());
        //    }
        //}
        // Debug.Log(tiles.Count);

        //Debug.Log(isDestroyingTiles);
    }

    public IEnumerator DestroyTilesWithDelay()
    {
        List<GameObject> tilesCopy = new List<GameObject>(tiles);
        

        foreach (var tile in tilesCopy)
        {
            isDestroyingTiles = true;
            if (tile != null && tiles.Contains(tile))
            {
                GameObject tilesBroken = Instantiate(tilesBrokenPrefab, tile.transform.position, tile.transform.rotation);
                if (tile != null)
                {
                    NetworkServer.Spawn(tilesBroken);

                    RpcexplodeTiles(tilesBroken,tile);
                    StartCoroutine(DestroyBreakableTileWithDelay(tilesBroken, tile));
              
                    yield return new WaitForSeconds(DestroyTilesSpeed);
        
                    tiles.Remove(tile);
                }
            }
        }
        PlayerMoveCamera[] playerMoveCamera = FindObjectsOfType<PlayerMoveCamera>();
        foreach(PlayerMoveCamera player in playerMoveCamera)
        {
            player.CmdSetPlayerHealth(player.setplayerLife);
        }

        timerScript.CmdStopCountdown();
        playerCount.CmdAnnounce();
        isDestroyingTiles = false;
        isActivated = false; 
        tiles.Clear();
    }

    [ClientRpc]
    void RpcexplodeTiles(GameObject tilesBroken, GameObject originalTile)
    {
        originalTile.GetComponent<BoxCollider>().enabled = false;
        originalTile.GetComponent<MeshRenderer>().enabled = false;
        ExplodeForce(tilesBroken, originalTile);
    }
    void ExplodeForce(GameObject tilesBroken, GameObject originalTile)
    {
      
        float explosionRadius = 5.0f;
        Vector3 explosionPosition = tilesBroken.transform.position;
        float rand = Random.Range(minexplosionForce, maxexplosionForce);

        foreach (Transform child in tilesBroken.transform)
        {
          
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //Debug.Log(rand);
                rb.AddExplosionForce(rand, explosionPosition, explosionRadius);
                //AudioSource sourceAudio = originalTile.GetComponent<AudioSource>();

                TileManager tileManager = originalTile.GetComponent<TileManager>();
                if (tileManager != null)
                {

                    tileManager.CmdPlayBreakSound();

                }

            }
          
        }
       
    }
    private IEnumerator DestroyBreakableTileWithDelay(GameObject objectToDestroy, GameObject originalTile)
    {
        yield return new WaitForSeconds(3f);

        if (objectToDestroy != null)
        {
            NetworkServer.Destroy(objectToDestroy);
            NetworkServer.Destroy(originalTile);
        }
    }
    public void SpawnTiles()
    {
        // activate.SetTrigger("isActivate");
        startedGame = true;
        isActivated = true;
        playerCount.CmdClearNames();
        for (int row = 0; row < rowCount; row++)
        {
            int breakableColumn = Random.Range(0, colCount);

            for (int col = 0; col < colCount; col++)
            {
                Vector3 spawnPosition = transform.position + new Vector3(col * spacing, 0, row * spacing);

                if (col == breakableColumn)
                {
                    tile = Instantiate(breakablePrefab, spawnPosition, Quaternion.identity);
                    tile.tag = "Breakable";
                   
                    NetworkServer.Spawn(tile);
                    RpcSetTag(tile, "Breakable");
                }
                else
                {
                    tile = Instantiate(breakablePrefab, spawnPosition, Quaternion.identity);
                    tile.tag = "Untagged";
                   
                    NetworkServer.Spawn(tile);
                    RpcSetTag(tile, "Untagged");
                }
                tiles.Add(tile);
            }
        }

    }
    [ClientRpc]
    void RpcSetTag(GameObject obj, string newTag)
    {
        obj.tag = newTag;
    }
    public void SpawnLights()
    {
      
        for (int i = 0; i < 200; i++)
        {
            float zoffset = i * 0.5f;
            Vector3 spawnPosition = spawnerLight.position + new Vector3(0, 0, zoffset);
            GameObject gameObject = Instantiate(lightSphere, spawnPosition, spawnerLight.rotation);
            Vector3 spawnPosition2 = spawnerLight2.position + new Vector3(0, 0, zoffset);
            GameObject gameObject2 = Instantiate(lightSphere, spawnPosition2, spawnerLight2.rotation);
            NetworkServer.Spawn(gameObject);
            NetworkServer.Spawn(gameObject2);
        }
       
    }
    public void ResetTiles()
    {
        //reset.SetTrigger("reset");
        PlayerMoveCamera[] playerMoveCamera = FindObjectsOfType<PlayerMoveCamera>();
        foreach (PlayerMoveCamera player in playerMoveCamera)
        {
            player.CmdSetPlayerHealth(player.setplayerLife);
        }
       
        playerCount.CmdClearNames();
        isActivated = false;
        startedGame = false;
      
       
        foreach (var tile in tiles)
        {
            if(tile != null)
                NetworkServer.Destroy(tile);
        }
       
        tiles.Clear();
    }
    public void RemoveTile(GameObject tileToRemove)
    {
        if (isServer)
        {
            tiles.Remove(tileToRemove);
        }
    }


}
