using System.Collections.Generic;
using UnityEngine;

public class BlockRenderer : MonoBehaviour
{
    ChunkManager chunkManagerInstance;
    public Object[] Blocks { get; private set; }
    BlockSide[,] blockSides;    //An array containing the side information of all blocks
    Vector3[] triangleNormals;
    List<Chunk> chunksToRemoveFromQueue;
    void Awake()
    {
        LoadAvailableBlocks();
    }
    void Start()
    {
        chunkManagerInstance = ChunkManager.Instance;
        GenerateChunkMesh();
    }
    void Update()
    {
        GenerateChunkMesh();
    }
    void GenerateChunkMesh()
    {
        chunksToRemoveFromQueue = new List<Chunk>();
        foreach (Chunk chunkToRender in chunkManagerInstance.ChunksToGenerate)
        {
            foreach (Transform existingChunk in chunkManagerInstance.transform)   //Iterate all existing chunks
            {
                if (existingChunk.name == ("Chunk " + chunkManagerInstance.GetChunkID(chunkToRender))) //The chunk has already been generated
                {
                    Destroy(existingChunk.gameObject);
                    break;
                }
            }

            //Create a chunk object
            GameObject g = new GameObject();
            g.transform.name = "Chunk " + chunkManagerInstance.GetChunkID(chunkToRender);
            g.transform.parent = chunkManagerInstance.transform;
            g.transform.position = chunkToRender.ChunkPosition;

            int blockID;
            //Iterate the virtual chunk
            for (int x = 0; x < chunkToRender.ChunkBlockIDs.GetLength(0); x++)
            {
                for (int y = 0; y < chunkToRender.ChunkBlockIDs.GetLength(1); y++)
                {
                    for (int z = 0; z < chunkToRender.ChunkBlockIDs.GetLength(2); z++)
                    {
                        if (chunkToRender.ChunkBlockIDs[x, y, z] == 0)  //Current block is air, nothing to render
                            continue;
                        blockID = chunkToRender.ChunkBlockIDs[x, y, z] - 1;     //-1 because actual block IDs start at 1, 0 is air
                        GameObject newCube = GameObject.Instantiate((GameObject)Blocks[blockID]);
                        newCube.transform.parent = g.transform;
                        newCube.transform.position = chunkManagerInstance.GetBlockPosition(chunkManagerInstance.GetChunkID(chunkToRender), new Vector3Int(x, y, z));
                    }
                }
            }
            chunksToRemoveFromQueue.Add(chunkToRender);
        }
        foreach(Chunk chunkToRemove in chunksToRemoveFromQueue)
        {
            chunkManagerInstance.ChunksToGenerate.Remove(chunkToRemove);
        }
    }
    void LoadAvailableBlocks()  //Load all the existing block prefabs
    {
        Blocks = Resources.LoadAll("Blocks",typeof(GameObject));
        System.Array.Sort(Blocks, (x, y) => ((GameObject)x).GetComponent<BlockProperties>().ID - ((GameObject)y).GetComponent<BlockProperties>().ID);
        blockSides = new BlockSide[Blocks.Length, 6]; //Each block has 6 possible directions its normals can face

        Mesh blockMesh;
        Vector3 point1, point2, point3;

        foreach (GameObject block in Blocks)
        {
            blockMesh = block.GetComponent<MeshFilter>().sharedMesh;
            triangleNormals = new Vector3[blockMesh.triangles.Length/3];

            //Get triangle normals
            for (int i = 0; i < triangleNormals.Length; i++)
            {
                //Get normals of the triangle vertices
                point1 = blockMesh.normals[blockMesh.triangles[i * 3]];
                point2 = blockMesh.normals[blockMesh.triangles[i * 3 + 1]];
                point3 = blockMesh.normals[blockMesh.triangles[i * 3 + 2]];

                //Calculate the triangle normal
                triangleNormals[i] = (point1 + point2 + point3) / 3;
            }

            //Separate the triangles
            int[] tempIndex;
            int y = 0;
            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = 0; j < 3; j++)
                {
                    tempIndex = new int[3];
                    tempIndex[j] = i;
                    Vector3 currentSide = new Vector3(tempIndex[0], tempIndex[1], tempIndex[2]);
                    y = ((int)(i / 2f + 0.5f) * 3) + j;

                    blockSides[System.Array.IndexOf(Blocks, block), y] = new BlockSide();

                    for (int k = 0; k < triangleNormals.Length; k++)
                    {
                        if (triangleNormals[k] == currentSide)
                        {
                            blockSides[System.Array.IndexOf(Blocks, block), y].triangles.Add(blockMesh.triangles[k * 3]);
                            blockSides[System.Array.IndexOf(Blocks, block), y].triangles.Add(blockMesh.triangles[k * 3 + 1]);
                            blockSides[System.Array.IndexOf(Blocks, block), y].triangles.Add(blockMesh.triangles[k * 3 + 2]);
                        }
                    }
                }
            }
        }
    }
}
