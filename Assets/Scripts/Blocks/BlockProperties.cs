using UnityEngine;

public class BlockProperties : MonoBehaviour
{
    public int ID;                  //ID of the block, count starts at 1, 0 is reserved for air (nothing)
    public string Name;             //Name of the block
    public float TimeToBreak;       //The time required to break the block in milliseconds
}
