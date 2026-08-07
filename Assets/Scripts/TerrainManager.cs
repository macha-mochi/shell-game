using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField] GameObject[] beachTiles;
    private float tileSideLength; // This is not only the scale of the tile but also the distance between them
    private int beachLayerIndex;
    private int indexPlayerOn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        beachLayerIndex = LayerMask.NameToLayer("TerrainTriggers");
        indexPlayerOn = 0;
        tileSideLength = beachTiles[0].transform.localScale.z;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == beachLayerIndex)
        {
            if (needNextTile())
            {
                loadNextTile();
            }
        }
    }

    // If we are on the tile that is farther on the z axis, then we need another one in front 
    bool needNextTile()
    {
        // Make sure indexPlayerOn is up to date

        if(getNumberLineCoord(transform.position.z) == getNumberLineCoord(beachTiles[0].transform.position.z))
        {
            indexPlayerOn = 0;
        } else if(getNumberLineCoord(transform.position.z) == getNumberLineCoord(beachTiles[1].transform.position.z))
        {
            indexPlayerOn = 1;
        } else
        {
            Debug.LogError("Player wandered out of bounds");
            return false; 
        }
        return beachTiles[indexPlayerOn].transform.position.z > beachTiles[1 - indexPlayerOn].transform.position.z;
    }
    void loadNextTile()
    {
        Vector3 nextPos = GetNextTilePosition();

        beachTiles[1 - indexPlayerOn].transform.position = nextPos;
    }
    // Make sure you call this before swapping the active tile index
    Vector3 GetNextTilePosition()
    {
        return beachTiles[indexPlayerOn].transform.position + Vector3.forward * tileSideLength;
    }
    // If the z axis were a number line divided into [tileSideLength] size cells, this is a world space position's loc on that number line
    int getNumberLineCoord(float z)
    {
        // [-tileSideLength/2, tileSideLength/2] should map to 0
        return (int)((z + tileSideLength/2) / tileSideLength);
    }
}

/*
Rule: first beach tile spawned at (0, 0, 0). when player is between [sidelength/2 and -sidelength/2] is at 0
beach tile i (0 indexed) should be at (0, 0, i * sideLength)
*/
