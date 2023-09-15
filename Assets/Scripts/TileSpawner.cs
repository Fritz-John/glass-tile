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

    public Animator activate;
    public Animator reset;
    GameObject tile;

    List<GameObject> tiles = new List<GameObject>();

    public TimerScript timer;
    public bool isDestroyingTiles = false;
    private int currentTileIndex = 0;
    public float destroyTimer = 1.0f;

    public GameObject tilesBrokenPrefab;

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
        if(timer.timer <= 0 )
        {
            if (!isDestroyingTiles)
            {
                isDestroyingTiles = true;
                StartCoroutine(DestroyTilesWithDelay());
            }
        }
        
    }

    private IEnumerator DestroyTilesWithDelay()
    {
        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                GameObject tilesBroken = Instantiate(tilesBrokenPrefab, tile.transform.position, tile.transform.rotation);   
                
                NetworkServer.Spawn(tilesBroken);

                RpcexplodeTiles(tilesBroken);
                StartCoroutine(DestroyBreakableTileWithDelay(tilesBroken));
                NetworkServer.Destroy(tile);
                yield return new WaitForSeconds(destroyTimer); // Wait for 1 second before destroying the next tile.
            }
        }
        isDestroyingTiles = false;
        tiles.Clear();
    }
 

    [ClientRpc]
    void RpcexplodeTiles(GameObject tilesBroken)
    {
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
    private IEnumerator DestroyBreakableTileWithDelay(GameObject objectToDestroy)
    {
        yield return new WaitForSeconds(3f);

        if (objectToDestroy != null)
        {
            NetworkServer.Destroy(objectToDestroy);
        }
    }
    public void SpawnTiles()
    {
        activate.SetTrigger("isActivate");
        isActivated = true;
        
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
        reset.SetTrigger("reset");
        isActivated = false;
    
        foreach (var tile in tiles)
        {
            if(tile != null)
                NetworkServer.Destroy(tile);

        }
        currentTileIndex = 0;
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
