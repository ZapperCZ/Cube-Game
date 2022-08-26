using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEditor : MonoBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField] float playerReach = 3;     //How far the player can reach
    [SerializeField] LayerMask blockLayer;

    public int[] ViewedBlockInfo;                   //Info about the block that is currently being looked at [chunkID,inChunkX,inChunkY,inChunkZ]

    ChunkManager chunkManagerInstance;
    RaycastHit hit;
    Vector3 targetBlockPosition = Vector3.zero;     //Position of the block to be placed / destroyed in global coordinates
    Chunk affectedChunk;                            //Chunk that the block is being placed / destroyed inside of
    int[] targetBlockInfo;
    int selectedBlockID = 1;                        //ID of the block to place, currently hardcoded
    float offset = 0.2f;                            //Offset from the face normal when getting a block
    float timer = 0f;                               //Internal timer for block breaking

    void Start()
    {
        chunkManagerInstance = ChunkManager.Instance;
    }
    void Update()
    {
        if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, playerReach, blockLayer)) //No blocks within reach
            return;
        ViewedBlockInfo = chunkManagerInstance.GetBlockAtPosition(hit.point);
        affectedChunk = chunkManagerInstance.ActiveChunks[ViewedBlockInfo[0]];
        if (Input.GetButtonDown("Left Click"))
        {

        }
        if (Input.GetButtonDown("Right Click"))
        {
            hit.transform.GetComponent<MeshRenderer>().material.color = Color.cyan;
            targetBlockInfo = chunkManagerInstance.GetBlockAtPosition(hit.point + (hit.normal * offset));
            PlaceBlock();
        }
    }
    void PlaceBlock()
    {
        chunkManagerInstance.ActiveChunks[targetBlockInfo[0]].ChunkBlockIDs[targetBlockInfo[1], targetBlockInfo[2], targetBlockInfo[3]] = selectedBlockID;
        chunkManagerInstance.ChunksToGenerate.Add(affectedChunk);
    }
    void BreakBlock()
    {
        chunkManagerInstance.ActiveChunks[targetBlockInfo[0]].ChunkBlockIDs[targetBlockInfo[1], targetBlockInfo[2], targetBlockInfo[3]] = 0;
        chunkManagerInstance.ChunksToGenerate.Add(affectedChunk);
    }
}
