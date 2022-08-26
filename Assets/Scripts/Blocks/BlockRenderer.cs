using System.Collections.Generic;
using UnityEngine;

public class BlockRenderer : MonoBehaviour
{
    ChunkManager chunkManagerInstance;
    Object[] blocks;
    BlockSide[,] blockSides;    //An array containing the side information of all blocks
    Vector3[] triangleNormals;
    void Start()
    {
        chunkManagerInstance = ChunkManager.Instance;
        LoadAvailableBlocks();
        //Test2DMeshGeneration();
        //Test3DMeshGeneration();
        GenerateChunkMesh();
    }
    void Test3DMeshGeneration()     //Generates a cube made out of a single mesh (Has broken lighting)
    {
        int blockID = 1;
        GameObject cube = new GameObject();
        cube.AddComponent<MeshRenderer>();
        cube.AddComponent<MeshFilter>();
        cube.GetComponent<MeshRenderer>().material = ((GameObject)blocks[blockID - 1]).GetComponent<MeshRenderer>().sharedMaterial;
        Mesh cubeMesh = new Mesh();
        Vector3 point1 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 point2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 point3 = new Vector3(-0.5f, 0.5f, -0.5f);
        Vector3 point4 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 point5 = new Vector3(-0.5f, 0.5f,0.5f);
        Vector3 point6 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 point7 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 point8 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3[] vertices = { point1, point2, point3, point4, point5, point6, point7, point8 };
        int[] triangles = { 7, 6, 2, 2, 4, 7, 5, 1, 6, 6, 7, 5, 1, 0, 2, 2, 6, 1, 3, 5, 7, 7, 4, 3, 3, 0, 1, 1, 5, 3, 4, 2, 0, 0, 3, 4, };
        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.RecalculateNormals();
        cube.GetComponent<MeshFilter>().mesh = cubeMesh;
    }
    void Test2DMeshGeneration() //Generates a cube with faces made out of separate meshes
    {
        int blockID = 1;    //Hardcoded ID of the block (Stone)
        int[] faceTris;
        Vector3[] tempVertices;
        GameObject generatedCube = new GameObject();
        generatedCube.transform.position = new Vector3(0, 5, 0);
        generatedCube.name = "Cube";
        for (int i = 0; i < blockSides.GetLength(1); i++)   //Iterate through all the sides of a block
        {
            faceTris = blockSides[blockID - 1, i].triangles.ToArray();  //Get all triangles from one side of the bock
            tempVertices = new Vector3[faceTris.Length];                //Temp array to store vertices that will be assigned to the mesh
            GameObject cubeFace = new GameObject();
            Mesh faceMesh = new Mesh();

            cubeFace.name = "Face " + i;
            cubeFace.transform.parent = generatedCube.transform;
            cubeFace.transform.localPosition = Vector3.zero;
            cubeFace.AddComponent<MeshFilter>();
            cubeFace.AddComponent<MeshRenderer>();
            cubeFace.GetComponent<MeshRenderer>().material = ((GameObject)blocks[blockID - 1]).GetComponent<MeshRenderer>().sharedMaterial;
            //Iterate through all the vertices that the triangles of this face hold IDs to
            for(int j = 0; j < faceTris.Length; j++)
            {
                tempVertices[j] = ((GameObject)blocks[blockID - 1]).GetComponent<MeshFilter>().sharedMesh.vertices[faceTris[j]];    //Get the vertex from the ID stored in the triangle and add it to a List
                faceTris[j] = j;
            }
            faceMesh.vertices = tempVertices;
            faceMesh.triangles = faceTris;
            faceMesh.RecalculateNormals();      //Fix lightning
            cubeFace.GetComponent<MeshFilter>().mesh = faceMesh;
        }
    }
    void GenerateChunkMesh()
    {
        foreach (Chunk chunkToRender in chunkManagerInstance.ChunksToGenerate)
        {
            //Temporary code for visible block generation
            //Create a chunk object
            GameObject g = new GameObject();
            g.transform.name = "Chunk " + chunkManagerInstance.GetChunkID(chunkToRender);
            g.transform.parent = chunkManagerInstance.transform;
            g.transform.position = chunkToRender.ChunkPosition;

            Mesh chunkMesh = new Mesh();
            int blockID;
            int faceID;
            int[] faceTris;
            Vector3[] tempVertices;
            //Iterate the virtual chunk
            for (int x = 0; x < chunkToRender.ChunkBlockIDs.GetLength(0); x++)
            {
                for (int y = 0; y < chunkToRender.ChunkBlockIDs.GetLength(1); y++)
                {
                    for (int z = 0; z < chunkToRender.ChunkBlockIDs.GetLength(2); z++)
                    {
                        if (chunkToRender.ChunkBlockIDs[x, y, z] == 0)  //Current block is air, nothing to render
                            break;
                        blockID = chunkToRender.ChunkBlockIDs[x, y, z] - 1; //-1 because actual block IDs start at 1, 0 is air
                        //Look through bordering blocks
                        for (int i = -1; i <= 1; i++)
                        {
                            for(int j = -1; j <= 1; j++)
                            {
                                for(int k = -1; k <= 1; k++)
                                {
                                    if (!IsInBounds(chunkToRender.ChunkBlockIDs, new Vector3Int(x + i, y + j, z + k)))      //Index is outside array bounds
                                        continue;
                                    if ((i*i) + (j*j) + (k*k) != 1)    //Bordering block is only bordering by its corner
                                        continue;
                                    if (chunkToRender.ChunkBlockIDs[x + i, y + j, z + k] != 0)  //Bordering block isn't air
                                        continue;
                                    faceID = (((int)((i + 1) * 1.5f)) * (i * i)) + (((int)((j + 1) * 1.5f)) + 1) * (j * j) + (((int)((k + 1) * 1.5f)) + 2) * (k * k);
                                    faceTris = blockSides[blockID, faceID].triangles.ToArray();
                                    tempVertices = new Vector3[faceTris.Length];
                                    for(int f = 0; f < faceTris.Length; f++)
                                    {
                                        tempVertices[f] = ((GameObject)blocks[blockID]).GetComponent<MeshFilter>().sharedMesh.vertices[faceTris[f]];
                                        faceTris[f] = f;
                                    }

                                    Debug.Log(i + " ," + j + " ," + k + " - " + faceID);
                                }
                            }
                        }
/*                        int[] faceTris;
                        Vector3[] tempVertices;
                        if (chunkToRender.ChunkBlockIDs[x, y, z] != 0)
                        {
                            faceTris = blockSides[chunkToRender.ChunkBlockIDs[x, y, z] - 1, 4].triangles.ToArray();
                            tempVertices = new Vector3[faceTris.Length];
                            GameObject cubeFace = new GameObject();
                            Mesh faceMesh = new Mesh();

                            cubeFace.name = "Face";
                            cubeFace.transform.parent = g.transform;
                            cubeFace.transform.localPosition = new Vector3(x - (chunkToRender.ChunkBlockIDs.GetLength(0) / 2), y - (chunkToRender.ChunkBlockIDs.GetLength(1) / 2), z - (chunkToRender.ChunkBlockIDs.GetLength(2) / 2));
                            cubeFace.AddComponent<MeshFilter>();
                            cubeFace.AddComponent<MeshRenderer>();
                            cubeFace.GetComponent<MeshRenderer>().material = ((GameObject)blocks[chunkToRender.ChunkBlockIDs[x, y, z] - 1]).GetComponent<MeshRenderer>().sharedMaterial;
                            for (int j = 0; j < faceTris.Length; j++)
                            {
                                tempVertices[j] = ((GameObject)blocks[chunkToRender.ChunkBlockIDs[x, y, z] - 1]).GetComponent<MeshFilter>().sharedMesh.vertices[faceTris[j]];
                                faceTris[j] = j;
                            }
                            faceMesh.vertices = tempVertices;
                            faceMesh.triangles = faceTris;
                            faceMesh.RecalculateNormals();
                            cubeFace.GetComponent<MeshFilter>().mesh = faceMesh;
                        }*/
                    }
                }
            }
        }
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
