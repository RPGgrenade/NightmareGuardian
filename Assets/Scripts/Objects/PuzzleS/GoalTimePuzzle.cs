using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTimePuzzle : PuzzleScript
{
    [Header("Goal References")]
    public bool StartEntered = false;
    public bool GoalEntered = false;

    [Header("Timer Properties")]
    [Tooltip("Whether or not it's currently timing the player")]
    public bool Timing = false;
    [Tooltip("How much time is left in the timer")]
    public float Timer = 0f;
    [Tooltip("How long the player needs to take to reach the goal")]
    public float MaxTime = 60f;

    void Update()
    {
        if (!Solved)
        {
            if (StartEntered && !Timing)
            {
                Timing = true;
                Invoke("stopTimer", MaxTime);
                //Activate sounds or particles to indicate start
            }
            else if (GoalEntered && Timing)
            {
                CancelInvoke("stopTimer");
                stopTimer();
                Solved = true;
                base.SpawnPaper();
            }
        }
    }

    private void stopTimer()
    {
        Timing = false;
        Timer = 0f;
    }
}
