using System.Collections.Generic;
using UnityEngine;

public class BlockRenderer : MonoBehaviour
{
    ChunkManager ChunkManagerInstance = ChunkManager.Instance;
    Object[] blocks;
    BlockSide[,] blockSides;    //An array containing the side information of all blocks
    Vector3[] triangleNormals;
    void Awake()
    {
        LoadAvailableBlocks();
    }
    void LoadAvailableBlocks()  //Load all the existing block prefabs
    {
        blocks = Resources.LoadAll("Blocks",typeof(GameObject));
        blockSides = new BlockSide[blocks.Length, 6]; //Each block has 6 possible directions its normals can face
        Mesh blockMesh;
        Vector3 point1, point2, point3;

        foreach (GameObject block in blocks)
        {
            blockMesh = block.GetComponent<MeshFilter>().sharedMesh;
            triangleNormals = new Vector3[blockMesh.triangles.Length/3];
            Debug.Log("Block ID - " + block.GetComponent<BlockProperties>().ID);

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
        }
    }
}
