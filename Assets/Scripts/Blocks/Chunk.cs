using UnityEngine;

public class Chunk
{
    public int[,,] ChunkBlockIDs = new int[32, 256, 32];        //The array containing IDs of all the blocks within the chunk
    public Vector3 ChunkPosition;                               //Global position of the chunk
}
