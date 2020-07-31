using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleScript : MonoBehaviour
{
    public enum PuzzleType
    {
        Place,
        Stab,
        Light,
        Cape,
        Grab,
        Grapple
    }

    //[Header("Puzzle Type")]
    //[Tooltip("The type of puzzle this is")]
    //public PuzzleType Puzzle;
    //[Space(15)]
    [Header("Aesthetic Paper Spawning Properties")]
    [Tooltip("Whether or not this puzzle's item has been unlocked yet, if solved, nothing happens if done again")]
    public bool Solved = false;
    [Tooltip("Type of aesthetic that's being unlocked")]
    public PaperItem.ItemType Type = PaperItem.ItemType.Hat;
    [Tooltip("Object to spawn when puzzle is solved")]
    public GameObject PaperSpawn;
    [Tooltip("Position offset of object to spawn in world space")]
    public Vector3 SpawnOffset;
    [Tooltip("Rotation offset of object to spawn in world space")]
    public Vector3 RotationOffset;
    [Tooltip("Item to show on object after spawn complete")]
    public Material MaterialItem;
    [Tooltip("Name of item to activate in the aesthetics collection")]
    public string ItemName;

    // Start is called before the first frame update
    void Start()
    {
        Solved = Saves.GetHatFound(ItemName);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject == ObjectToPlace && !Solved)
    //    {
    //        Solved = true;
    //        SpawnPaper();
    //    }
    //}

    public void SpawnPaper()
    {
        GameObject paper = GameObject.Instantiate(PaperSpawn);
        paper.transform.position = this.transform.position + this.transform.TransformDirection(SpawnOffset);
        paper.transform.rotation = Quaternion.Euler(RotationOffset);
        paper.GetComponent<PaperItem>().SetItem(MaterialItem);
        paper.GetComponent<PaperItem>().SetName(ItemName);
        paper.GetComponent<PaperItem>().SetType(Type);
    }
}
