using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] int renderDistance = 6;                    //How many chunks should render near the player
    [SerializeField] List<Chunk> chunks = new List<Chunk>();    //List of all chunks in the level
    public List<Chunk> ActiveChunks = new List<Chunk>();        //List of all the currently loaded chunks

    private void Start()
    {
        ManuallyPopulateChunks();
        foreach(Chunk c in chunks)
        {
            Debug.Log(c.ChunkPosition);
        }
        LoadNearbyChunks();
    }
    void ManuallyPopulateChunks()   //A debug method to manually create chunks and add blocks into them
    {
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;           //Skip the middle - a rigid platform is in place instead

                Chunk newChunk = new Chunk();
                newChunk.ChunkPosition = new Vector3(i * 32, j * 32, 0);

                //Create a stone layer in the middle of the chunk
                for(int x = 0; x < newChunk.ChunkBlockIDs.GetLength(0); x++)
                {
                    for (int y = 0; y < newChunk.ChunkBlockIDs.GetLength(1); y++)
                    {
                        newChunk.ChunkBlockIDs[x, y, (newChunk.ChunkBlockIDs.GetLength(2)) / 2] = 1;
                    }
                }
                chunks.Add(newChunk);
            }
        }
    }
    void LoadNearbyChunks() //Loads nearby chunks
    {

    }
    void UnloadChunks()     //Unloads all the chunks
    {
        
    }
    public Chunk GetChunkAtPosition(Vector3 position)  //Returns the chunk that is closest to the input position
    {
        Chunk closestChunk = null;
        float currentDistance, lastDistance;
        lastDistance = 0;
        foreach (Chunk c in chunks)
        {
            currentDistance = Vector3.Distance(position, c.ChunkPosition);      //cache the calculated distance to offload work from the cpu
            if (currentDistance < lastDistance)
            {
                lastDistance = currentDistance;
                closestChunk = c;
            }
        }
        return closestChunk;
    }
    public int[] GetBlockAtPosition(Vector3 position)      //Returns the chunk coordinates of a block closest to the input position [chunk ID in the chunks list, block x, block y, block z]
    {
        int[] result = new int[4];

        int blockChunkID = chunks.IndexOf(GetChunkAtPosition(position));    //ID of the chunk that the block is in

        Vector3 localPosition = position - chunks[blockChunkID].ChunkPosition;  
        int blockX = Mathf.RoundToInt(localPosition.x);
        int blockY = Mathf.RoundToInt(localPosition.y);
        int blockZ = Mathf.RoundToInt(localPosition.z);

        result[0] = blockChunkID;
        result[1] = blockX;
        result[2] = blockY;
        result[3] = blockZ;

        return result;
    }
    public Vector3 GetBlockPosition(int chunkID, Vector3 blockCoordinates) //Returns global coordinates of a block in a chunk at the specified coordinates
    {
        Vector3 blockPosition = blockCoordinates + chunks[chunkID].ChunkPosition;
        return blockPosition;
    }
}
