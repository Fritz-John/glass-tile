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

    void Start()
    {
        if (isServer)
        {
            SpawnTiles();
        }
    }

    void SpawnTiles()
    {
        for (int row = 0; row < rowCount; row++)
        {
            int breakableColumn = Random.Range(0, colCount);

            for (int col = 0; col < colCount; col++)
            {
                Vector3 spawnPosition = transform.position + new Vector3(col * spacing, 0, row * spacing);

                if (col == breakableColumn)
                {
                    GameObject tile = Instantiate(breakablePrefab, spawnPosition, Quaternion.identity);
                    NetworkServer.Spawn(tile); 
                }
                else
                {
                    GameObject tile = Instantiate(nonBreakablePrefab, spawnPosition, Quaternion.identity);
                    NetworkServer.Spawn(tile); 
                }
            }
        }
    }
}
