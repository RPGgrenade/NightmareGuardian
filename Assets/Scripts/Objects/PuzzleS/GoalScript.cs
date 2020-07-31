using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScript : MonoBehaviour
{
    public GoalTimePuzzle Puzzle;
    public bool IsGoal = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.tag == "Player" && 
            other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (IsGoal)
                Puzzle.GoalEntered = true;
            else
                Puzzle.StartEntered = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.tag == "Player" &&
            other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (IsGoal)
                Puzzle.GoalEntered = true;
            else
                Puzzle.StartEntered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.tag == "Player" &&
            other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (IsGoal)
                Puzzle.GoalEntered = false;
            else
                Puzzle.StartEntered = false;
        }
    }
}
