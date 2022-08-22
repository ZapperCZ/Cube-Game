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
        LoadNearbyChunks();
    }
    void ManuallyPopulateChunks()   //A debug method to manually create chunks and add blocks into them
    {

    }
    void LoadNearbyChunks() //Loads nearby chunks
    {

    }
    void UnloadChunks()     //Unloads all the chunks
    {
        
    }
    Chunk GetNearestChunk(Vector3 position)  //Returns the chunk that is closest to the input position
    {
        Chunk closestChunk = null;
        float currentDistance, lastDistance;
        lastDistance = 0;
        foreach(Chunk c in chunks)
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
}
