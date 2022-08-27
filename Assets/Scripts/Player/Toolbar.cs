using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Toolbar : MonoBehaviour
{
    [Header("Toolbar Settings")]
    [SerializeField] int[] placeableBlockIDs = { 2, 3, 4, 5, 6 };
    [SerializeField] string inputBlockSelectionPrefix = "Select Block "; 
    [SerializeField] int maxPlaceableBlocks = 5;         //Maximum amount of placeable blocks, this is because the toolbar isn't responsive to block amount
    [SerializeField] Color selectedBlockBackgroundColor;
    [SerializeField] Color defaultBackgroundColor;
    [Header ("UI References")]
    [SerializeField] TextMeshProUGUI selectedBlockText;
    [SerializeField] GameObject CanvasParent;

    BlockRenderer playerBlockRenderer;
    BlockEditor playerBlockEditor;
    List<Image> blockThumbnailBackgrounds;
    string builderString = "";
    bool selectedBlockChanged = true;

    void Start()
    {
        playerBlockRenderer = transform.GetComponent<BlockRenderer>();
        playerBlockEditor = transform.GetComponent<BlockEditor>();
        if (placeableBlockIDs.Length > maxPlaceableBlocks)
        {
            TrimPlaceableBlockArray();
        }
        LoadToolbarGraphics();
    }

    void Update()
    {
        HandleInput();
        UpdateToolbarGraphics();
    }
    void HandleInput()  //Reads the player input and selects the according block
    {
        for (int i = 0; i < placeableBlockIDs.Length; i++)
        {
            builderString = inputBlockSelectionPrefix + (i + 1);
            if (Input.GetButtonDown(builderString))
            {
                playerBlockEditor.selectedBlockID = placeableBlockIDs[i];
                selectedBlockChanged = true;
            }
        }
    }
    void UpdateToolbarGraphics()    //Updates background of blocks on the toolbar to match the selected block
    {
        if (selectedBlockChanged)
        {
            selectedBlockChanged = false;
            for (int i = 0; i < blockThumbnailBackgrounds.Count; i++)
            {
                blockThumbnailBackgrounds[i].color = defaultBackgroundColor;
            }
            blockThumbnailBackgrounds[System.Array.IndexOf(placeableBlockIDs, playerBlockEditor.selectedBlockID)].color = selectedBlockBackgroundColor;
        }
        selectedBlockText.text = ((GameObject)playerBlockRenderer.Blocks[playerBlockEditor.selectedBlockID - 1]).GetComponent<BlockProperties>().Name;
    }
    void LoadToolbarGraphics()      //Generates block graphics and loads block backgrounds
    {
        RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
        Texture2D tempTexture;
        List<Image> blockThumbnails = new List<Image>();
        blockThumbnailBackgrounds = new List<Image>();
        //Current loading system depends on the blocks being correctly sorted in hierarchy in a descending order - Loading order is the index
        foreach (Transform child in CanvasParent.transform)
        {
            if (!HasComponent<Image>(child.gameObject) || !child.gameObject.name.Contains("Block"))
                continue;
            if (child.gameObject.name.Contains("Background"))
            {
                blockThumbnailBackgrounds.Add(child.GetComponent<Image>());
                continue;
            }
            if (HasComponent<Image>(child.gameObject) && !child.gameObject.name.Contains("Background"))
                blockThumbnails.Add(child.GetComponent<Image>());
        }
        for (int i = 0; i < placeableBlockIDs.Length; i++)
        {
            tempTexture = RuntimePreviewGenerator.GenerateModelPreview(((GameObject)playerBlockRenderer.Blocks[placeableBlockIDs[i]-1]).transform,256,256);
            blockThumbnails[i].sprite = Sprite.Create(tempTexture, new Rect(0, 0, tempTexture.width, tempTexture.height), new Vector2(tempTexture.width / 2, tempTexture.height / 2));
        }
    }
    void TrimPlaceableBlockArray()  //Trims the array of placeable blocks if it contains more blocks than the set maximum (Toolbar graphics and input system need to be changed beforehand)
    {
        int[] tempArray = new int[maxPlaceableBlocks];
        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = placeableBlockIDs[i];
        }
        placeableBlockIDs = tempArray;
    }
    bool HasComponent<T>(GameObject inputObject) where T : Component //Returns whether the input object has a specified component or not
    {
        return inputObject.GetComponent<T>() != null;
    }
}
