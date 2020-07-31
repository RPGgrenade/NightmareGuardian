using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectKeyPuzzle : PuzzleScript
{
    [Header("Place Puzzle Properties")]
    [Tooltip("Object that needs to enter a trigger area in order to solve the puzzle")]
    public GameObject ObjectToPlace;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ObjectToPlace && !Solved)
        {
            Solved = true;
            base.SpawnPaper();
        }
    }
}
