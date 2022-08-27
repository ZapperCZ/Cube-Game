using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI selectedBlockText;
    [SerializeField] int[] placeableBlockIDs = { 2, 3, 4, 5, 6 };
    [SerializeField] string blockSelectionPrefix = "Select Block "; 
    [SerializeField] int maxPlaceableBlocks = 5;         //Maximum amount of placeable blocks, this is because the toolbar isn't responsive to block amount

    BlockRenderer playerBlockRenderer;
    BlockEditor playerBlockEditor;
    string builderString = "";

    void Start()
    {
        playerBlockRenderer = transform.GetComponent<BlockRenderer>();
        playerBlockEditor = transform.GetComponent<BlockEditor>();
        if(placeableBlockIDs.Length > maxPlaceableBlocks)
        {
            int[] tempArray = new int[maxPlaceableBlocks];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = placeableBlockIDs[i];
            }
            placeableBlockIDs = tempArray;
        }
    }

    void Update()
    {
        //Select a block
        for(int i = 0; i < placeableBlockIDs.Length; i++)
        {
            builderString = blockSelectionPrefix + (i + 1);
            if (Input.GetButtonDown(builderString))
            {
                playerBlockEditor.selectedBlockID = placeableBlockIDs[i];
            }
        }
        selectedBlockText.text = ((GameObject)playerBlockRenderer.Blocks[playerBlockEditor.selectedBlockID - 1]).GetComponent<BlockProperties>().Name;
    }
}
