using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Telepathy;

public class TileSpawner : NetworkBehaviour
{
    public GameObject breakablePrefab;
    public GameObject nonBreakablePrefab;
    public float spacing = 7f;

    public int rowCount = 10;
    public int colCount = 2;

    public static TileSpawner instance;

    [SyncVar]
    public bool isActivated = false;

    //public Animator activate;
    //public Animator reset;
    GameObject tile;

    List<GameObject> tiles = new List<GameObject>();
    
    public TimerScript timer;
    public bool isDestroyingTiles = false;
 
    public float destroyTimer = 1.0f;
    public GameObject tilesBrokenPrefab;

    public TimerScript timerScript;
    public PlayerCounter playerCount;
    public bool startedGame = false;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (isServer)
        {
           // SpawnTiles();
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
    }

    public IEnumerator DestroyTilesWithDelay()
    {
        List<GameObject> tilesCopy = new List<GameObject>(tiles);
        

        foreach (var tile in tilesCopy)
        {
          
            if (tile != null && tiles.Contains(tile))
            {
                GameObject tilesBroken = Instantiate(tilesBrokenPrefab, tile.transform.position, tile.transform.rotation);
                if (tile != null)
                {
                    NetworkServer.Spawn(tilesBroken);

                    RpcexplodeTiles(tilesBroken,tile);
                    StartCoroutine(DestroyBreakableTileWithDelay(tilesBroken, tile));
              
                    yield return new WaitForSeconds(destroyTimer);
        
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
        ExplodeForce(tilesBroken);
    }
    void ExplodeForce(GameObject tilesBroken)
    {     
        float explosionForce = 200f;
        float explosionRadius = 5.0f;
        Vector3 explosionPosition = tilesBroken.transform.position;

        foreach (Transform child in tilesBroken.transform)
        {
          
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
                TileManager tileManager = FindObjectOfType<TileManager>();
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
        playerCount.ClearNames();
        for (int row = 0; row < rowCount; row++)
            {
                int breakableColumn = Random.Range(0, colCount);

                for (int col = 0; col < colCount; col++)
                {
                    Vector3 spawnPosition = transform.position + new Vector3(col * spacing, 0, row * spacing);

                    if (col == breakableColumn)
                    {
                        tile = Instantiate(breakablePrefab, spawnPosition, Quaternion.identity);
                        NetworkServer.Spawn(tile);
                       
                    }
                    else
                    {
                        tile = Instantiate(nonBreakablePrefab, spawnPosition, Quaternion.identity);
                        NetworkServer.Spawn(tile);
                        
                    }
                tiles.Add(tile);
            }
            }
        
    }

    public void ResetTiles()
    {
        //reset.SetTrigger("reset");
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
