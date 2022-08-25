using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }   //The Singleton Instance of this script

    [SerializeField] float chunkUpdateInterval = 5;             //How often should the ActiveChunk list be updated (in seconds)
    [SerializeField] int renderDistance = 6;                    //How many chunks should render near the player - number determines how many chunks away the player can see
    [SerializeField] GameObject DebugBlock;
    [SerializeField] GameObject Player;                         //The active player character
    List<Chunk> chuks = new List<Chunk>();                      //List of all chunks in the level
    List<Chunk> ActiveChunks = new List<Chunk>();        //List of all the currently loaded chunks
    public List<Chunk> ChunksToGenerate = new List<Chunk>();    //List of chunks that need to have their mesh regenerated
    public bool PlayerMoved = false;

    Chunk lastPlayerChunk = null;
    float timer = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)   //There already exists an instance of this manager
        {
            Destroy(this);  //Destroy the new instance (this one)
            return;         //Stop further code execution
        }
        else
        {
            Instance = this;    //No instance exists yet, set the instance reference to this one
        }
    }
    private void Start()
    {
        ManuallyPopulateChunks();       //Create the chunks and populate them with virtual blocks
        LoadNearbyChunks();
        GetChunksToUpdate();
    }
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > chunkUpdateInterval)
        {
            timer = 0;
            LoadNearbyChunks();
            GetChunksToUpdate();
        }
    }
    void ManuallyPopulateChunks()   //A debug method to manually create chunks and add blocks into them
    {
        //Create a 3x3 chunk grid with the center one being at [0;0;0]
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Chunk newChunk = new Chunk();
                newChunk.ChunkPosition = new Vector3(i * newChunk.ChunkBlockIDs.GetLength(0), 0, j * newChunk.ChunkBlockIDs.GetLength(2));

                //Create a stone layer in the middle of the chunk
                for (int x = 0; x < newChunk.ChunkBlockIDs.GetLength(0); x++)
                {
                    for (int z = 0; z < newChunk.ChunkBlockIDs.GetLength(2); z++)
                    {
                        newChunk.ChunkBlockIDs[x, (newChunk.ChunkBlockIDs.GetLength(1)) / 2, z] = 1;
                    }
                }
                chuks.Add(newChunk);
            }
        }
    }
    void GetChunksToUpdate()
    {
        bool chunkExists;
        //TODO: Change this into a Lambda expression to improve efficiency
        foreach (Chunk activeChunk in ActiveChunks)
        {
            chunkExists = false;
            foreach(Transform existingChunk in transform)   //Iterate all existing chunks
            {
                if (existingChunk.name == ("Chunk " + chuks.IndexOf(activeChunk))) //The chunk has already been generated
                {
                    chunkExists = true;
                    break;
                }
            }
            if (!chunkExists && !ChunksToGenerate.Contains(activeChunk))    //The chunk hasn't been generated yet and isn't queued for generation
            {
                ChunksToGenerate.Add(activeChunk);
            }
        }
    }
    void LoadNearbyChunks() //Loads nearby chunks and unloads the far away ones
    {
        Chunk playerChunk = GetChunkAtPosition(Player.transform.position);      //The Chunk that the player is currently located in
        Chunk chunkToLoad;
        int chunkWidth = playerChunk.ChunkBlockIDs.GetLength(0);
        if (playerChunk == lastPlayerChunk)         //The player hasn't moved from the current chunk since the last update, there is no need to laod the chunks again
        {
            PlayerMoved = false;
            return;
        }
        PlayerMoved = true;

        //Load the chunks that are nearby
        //TODO: Optimize the radius creation. Currently a square is loaded and then cut down to a circle - more efficient would be to first unload the chunks and then load the circle directly
        for (int i = ((int)playerChunk.ChunkPosition.x) - (chunkWidth * renderDistance); i <= ((int)playerChunk.ChunkPosition.x) + (chunkWidth * renderDistance); i += chunkWidth)
        {
            for (int j = ((int)playerChunk.ChunkPosition.y) - (chunkWidth * renderDistance); j <= ((int)playerChunk.ChunkPosition.y) + (chunkWidth * renderDistance); j += chunkWidth)
            {
                chunkToLoad = GetChunkAtPosition(new Vector3(i, 0, j));
                if (!ActiveChunks.Contains(chunkToLoad))
                {
                    ActiveChunks.Add(chunkToLoad);
                }
            }
        }

        //Unload the chunks that are far away
        //Iterate the list backwards to remove from it while looping
        for (int i = ActiveChunks.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(ActiveChunks[i].ChunkPosition, Player.transform.position) > chunkWidth * renderDistance)
            {
                ActiveChunks.RemoveAt(i);
            }
        }
        /*
        foreach(Chunk c in ActiveChunks)
        {
            Debug.Log(chunks.IndexOf(c));
        }
        */
    }
    public int GetChunkID(Chunk chunk)  //Returns ID of the input chunk in the chunk array; avoids exposing the chunk array
    {
        return chuks.IndexOf(chunk);
    }
    public Chunk GetChunkAtPosition(Vector3 position)  //Returns the chunk that is closest to the input position
    {
        Chunk closestChunk = null;
        float currentDistance, lastDistance;
        lastDistance = int.MaxValue;
        foreach (Chunk c in chuks)
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

        int blockChunkID = chuks.IndexOf(GetChunkAtPosition(position));    //ID of the chunk that the block is in

        Vector3 localPosition = position - chuks[blockChunkID].ChunkPosition;  
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
        Chunk blockChunk = chuks[chunkID];
        Vector3 blockPosition = blockCoordinates + blockChunk.ChunkPosition - new Vector3(blockChunk.ChunkBlockIDs.GetLength(0)/2, chuks[chunkID].ChunkBlockIDs.GetLength(1)/2, blockChunk.ChunkBlockIDs.GetLength(2) / 2);     //Vertical offset to make the middle of the array sit at 0 vertical
        return blockPosition;
    }
}
