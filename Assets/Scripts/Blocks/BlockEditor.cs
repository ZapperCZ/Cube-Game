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
    Material originalBlockMaterial;
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
/*                Debug.Log("Changed");
                if (targetBlockID != 0 && lastViewedBlock != null)
                {
                    lastViewedBlock.GetComponent<MeshRenderer>().material = originalBlockMaterial;
                }
                lastViewedBlock = hit.transform.gameObject;
                originalBlockMaterial = lastViewedBlock.GetComponent<MeshRenderer>().material;
                lastViewedBlock.GetComponent<MeshRenderer>().material.color = Color.red;*/
                
                targetBlockInfo = ViewedBlockInfo;
                targetBlockID = viewedBlockID;
                affectedChunk = chunkManagerInstance.ActiveChunks[targetBlockInfo[0]];
                timer = 0;
            }
            timer += Time.deltaTime;
            //Debug.Log(timer +" - "+ playerBlockRenderer.GetBlock(viewedBlockID - 1).GetComponent<BlockProperties>().TimeToBreak / 1000);
            if (timer > playerBlockRenderer.GetBlock(viewedBlockID-1).GetComponent<BlockProperties>().TimeToBreak/1000)
            {
                BreakBlock();
                timer = 0;
            }
        }
/*        if(Input.GetButtonUp("Left Click") && lastViewedBlock != null && lastViewedBlockID != 0)
        {
            lastViewedBlock.GetComponent<MeshRenderer>().material = playerBlockRenderer.GetBlock(lastViewedBlockID - 1).GetComponent<MeshRenderer>().sharedMaterial;
        }*/
        if (Input.GetButtonDown("Right Click"))
        {
            hit.transform.GetComponent<MeshRenderer>().material.color = Color.cyan;
            targetBlockInfo = chunkManagerInstance.GetBlockAtPosition(hit.point + (hit.normal * offset));
            affectedChunk = chunkManagerInstance.ActiveChunks[targetBlockInfo[0]];
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
