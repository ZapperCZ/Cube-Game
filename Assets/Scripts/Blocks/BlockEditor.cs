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
    BlockRenderer playerBlockRenderer;
    RaycastHit hit;
    Chunk affectedChunk;                            //Chunk that the block is being placed / destroyed inside of
    int[] targetBlockInfo;
    int targetBlockID = 0;
    int viewedBlockID = 0;
    Color originalBlockColor;
    GameObject lastViewedBlock = null;
    int selectedBlockID = 1;                        //ID of the block to place, currently hardcoded
    float offset = 0.2f;                            //Offset from the face normal when getting a block
    float timer = 0f;                               //Internal timer for block breaking

    void Start()
    {
        chunkManagerInstance = ChunkManager.Instance;
        playerBlockRenderer = transform.GetComponent<BlockRenderer>();
        targetBlockInfo = new int[4];
    }
    void Update()
    {
        if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, playerReach, blockLayer)) //No blocks within reach
            return;
        ViewedBlockInfo = chunkManagerInstance.GetBlockAtPosition(hit.point + (hit.normal * offset * -1));
        viewedBlockID = chunkManagerInstance.ActiveChunks[ViewedBlockInfo[0]].ChunkBlockIDs[ViewedBlockInfo[1], ViewedBlockInfo[2], ViewedBlockInfo[3]];
        
        if (Input.GetButton("Left Click") && viewedBlockID!=0)
        {
            if (targetBlockInfo[0] != ViewedBlockInfo[0] ||
                targetBlockInfo[1] != ViewedBlockInfo[1] ||
                targetBlockInfo[2] != ViewedBlockInfo[2] ||
                targetBlockInfo[3] != ViewedBlockInfo[3])      //The block being broken changed
            {
                //Color highlight
                if (targetBlockID != 0 && lastViewedBlock != null)
                {
                    lastViewedBlock.GetComponent<MeshRenderer>().material.color = originalBlockColor;
                }
                lastViewedBlock = hit.transform.gameObject;
                //Copy the color values manually to avoid creating a reference to the original which gets altered later
                originalBlockColor.a = lastViewedBlock.GetComponent<MeshRenderer>().material.color.a;
                originalBlockColor.r = lastViewedBlock.GetComponent<MeshRenderer>().material.color.r;
                originalBlockColor.g = lastViewedBlock.GetComponent<MeshRenderer>().material.color.g;
                originalBlockColor.b = lastViewedBlock.GetComponent<MeshRenderer>().material.color.b;
                lastViewedBlock.GetComponent<MeshRenderer>().material.color = Color.red;

                targetBlockInfo = ViewedBlockInfo;
                targetBlockID = viewedBlockID;
                affectedChunk = chunkManagerInstance.ActiveChunks[targetBlockInfo[0]];
                timer = 0;
            }
            timer += Time.deltaTime;
            if (timer > playerBlockRenderer.GetBlock(viewedBlockID-1).GetComponent<BlockProperties>().TimeToBreak/1000)
            {
                BreakBlock();
                timer = 0;
            }
        }
        if (Input.GetButtonUp("Left Click") && targetBlockID != 0 && lastViewedBlock != null)
        {
            lastViewedBlock.GetComponent<MeshRenderer>().material.color = originalBlockColor;
            targetBlockInfo = new int[4];
        }
        if (Input.GetButtonDown("Right Click"))
        {
            //Color highlight
            hit.transform.GetComponent<MeshRenderer>().material.color = Color.cyan;

            targetBlockInfo = chunkManagerInstance.GetBlockAtPosition(hit.point + (hit.normal * offset));
            affectedChunk = chunkManagerInstance.ActiveChunks[targetBlockInfo[0]];
            PlaceBlock();
            targetBlockInfo = new int[4];
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
