using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TileSpawner : NetworkBehaviour
{
    public GameObject breakablePrefab;
    public GameObject nonBreakablePrefab;
    public float spacing = 7f;

    public int rowCount = 10;
    public int colCount = 2;

    public static TileSpawner instance;
    public bool isActivated = false;

    public Animator activate;
    public Animator reset;
    GameObject tile;
    List<GameObject> tiles = new List<GameObject>();
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
            NetworkServer.Destroy(tile);

        }

        // Clear the list to remove references to the destroyed tiles
        tiles.Clear();
    }

   
}
