using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }   //The Singleton Instance of this script

    [SerializeField] float chunkUpdateInterval = 5;             //How often should the ActiveChunk list be updated (in seconds)
    [SerializeField] int renderDistance = 6;                    //How many chunks should render near the player - number determines how many chunks away the player can see
    [SerializeField] List<Chunk> chunks = new List<Chunk>();    //List of all chunks in the level
    [SerializeField] GameObject DebugBlock;
    [SerializeField] GameObject Player;                         //The active player character
    public List<Chunk> ActiveChunks = new List<Chunk>();        //List of all the currently loaded chunks
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

        //Temporary code for visible block generation
        foreach (Chunk c in chunks)
        {
            //Create a chunk object
            GameObject g = new GameObject();
            g.transform.name = "Chunk " + chunks.IndexOf(c);
            g.transform.parent = transform;
            g.transform.position = c.ChunkPosition;

            //Iterate the virtual chunk
            for (int x = 0; x < c.ChunkBlockIDs.GetLength(0); x++)
            {
                for (int y = 0; y < c.ChunkBlockIDs.GetLength(1); y++)
                {
                    for (int z = 0; z < c.ChunkBlockIDs.GetLength(2); z++)
                    {
                        //Instantiate blocks where virtual blocks are present
                        if (c.ChunkBlockIDs[x, y, z] == 1)
                        {
                            GameObject newCube = GameObject.Instantiate(DebugBlock);
                            newCube.transform.parent = g.transform;
                            newCube.transform.position = GetBlockPosition(chunks.IndexOf(c), new Vector3(x, y, z));
                        }
                    }
                }
            }
        }
        LoadNearbyChunks();
    }
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > chunkUpdateInterval)
        {
            timer = 0;
            LoadNearbyChunks();
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
                chunks.Add(newChunk);
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
    public Chunk GetChunkAtPosition(Vector3 position)  //Returns the chunk that is closest to the input position
    {
        Chunk closestChunk = null;
        float currentDistance, lastDistance;
        lastDistance = int.MaxValue;
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
        Chunk blockChunk = chunks[chunkID];
        Vector3 blockPosition = blockCoordinates + blockChunk.ChunkPosition - new Vector3(blockChunk.ChunkBlockIDs.GetLength(0)/2, chunks[chunkID].ChunkBlockIDs.GetLength(1)/2, blockChunk.ChunkBlockIDs.GetLength(2) / 2);     //Vertical offset to make the middle of the array sit at 0 vertical
        return blockPosition;
    }
}
