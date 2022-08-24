using System.Collections.Generic;
using UnityEngine;

public class BlockRenderer : MonoBehaviour
{
    ChunkManager ChunkManagerInstance = ChunkManager.Instance;
    Object[] blocks;
    Mesh[,] blockSides;
    void Awake()
    {
        LoadAvailableBlocks();
    }
    void LoadAvailableBlocks()  //Load all the existing block prefabs
    {
        blocks = Resources.LoadAll("Blocks",typeof(GameObject));
        foreach(GameObject block in blocks)
        {
            Debug.Log(block.GetComponent<BlockProperties>().ID);
        }

        blockSides = new Mesh[blocks.Length, 6]; //Each block has 6 possible directions its UVs can face
    }
}
