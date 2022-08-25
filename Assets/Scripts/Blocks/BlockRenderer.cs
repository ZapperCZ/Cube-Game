using System.Collections.Generic;
using UnityEngine;

public class BlockRenderer : MonoBehaviour
{
    ChunkManager ChunkManagerInstance;
    Object[] blocks;
    BlockSide[,] blockSides;    //An array containing the side information of all blocks
    Vector3[] triangleNormals;
    void Start()
    {
        ChunkManagerInstance = ChunkManager.Instance;
        LoadAvailableBlocks();
        TestMeshGeneration();
        //GenerateChunkMesh();
    }
    void TestMeshGeneration()
    {
        int blockID = 1;
        List<Vector3> tempVertices;
        int[] faceTris;
        GameObject generatedCube = new GameObject();
        generatedCube.transform.position = new Vector3(0, 5, 0);
        generatedCube.name = "Cube";
        for (int i = 0; i < blockSides.GetLength(1); i++)
        {
            tempVertices = new List<Vector3>();
            faceTris = blockSides[blockID - 1, i].triangles.ToArray();
            GameObject cubeFace = new GameObject();
            Mesh faceMesh = new Mesh();

            cubeFace.name = "Face " + i;
            cubeFace.transform.parent = generatedCube.transform;
            cubeFace.transform.localPosition = Vector3.zero;
            cubeFace.AddComponent<MeshFilter>();
            cubeFace.AddComponent<MeshRenderer>();
            cubeFace.GetComponent<MeshRenderer>().material = ((GameObject)blocks[blockID - 1]).GetComponent<MeshRenderer>().sharedMaterial;
            for(int j = 0; j < faceTris.Length; j++)
            {
                tempVertices.Add(((GameObject)blocks[blockID - 1]).GetComponent<MeshFilter>().sharedMesh.vertices[faceTris[j]]);
                faceTris[j] = j;
            }
            faceMesh.vertices = tempVertices.ToArray();
            faceMesh.triangles = faceTris;
            faceMesh.RecalculateNormals();
            cubeFace.GetComponent<MeshFilter>().mesh = faceMesh;
        }
    }
    void GenerateChunkMesh()
    {

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

                    blockSides[System.Array.IndexOf(blocks, block), y] = new BlockSide();

                    for (int k = 0; k < triangleNormals.Length; k++)
                    {
                        if (triangleNormals[k] == currentSide)
                        {
                            blockSides[System.Array.IndexOf(blocks, block), y].triangles.Add(blockMesh.triangles[k * 3]);
                            blockSides[System.Array.IndexOf(blocks, block), y].triangles.Add(blockMesh.triangles[k * 3 + 1]);
                            blockSides[System.Array.IndexOf(blocks, block), y].triangles.Add(blockMesh.triangles[k * 3 + 2]);
                        }
                    }
                }
            }
        }
    }
    bool IsInBounds(int[,,] arrayToCheck, Vector3Int positionToCheck)
    {
        bool result = false;
        result = (positionToCheck.x >= 0 && positionToCheck.x < arrayToCheck.GetLength(0)) &&
                (positionToCheck.y >= 0 && positionToCheck.y < arrayToCheck.GetLength(1)) &&
                (positionToCheck.z >= 0 && positionToCheck.z < arrayToCheck.GetLength(2));
        return result;
    }
}
