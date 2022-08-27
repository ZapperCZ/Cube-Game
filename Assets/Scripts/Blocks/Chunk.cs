using UnityEngine;

public class Chunk
{
    public int[,,] ChunkBlockIDs = new int[16, 128, 16];        //The array containing IDs of all the blocks within the chunk
    public Vector3 ChunkPosition;                               //Global position of the chunk
}
