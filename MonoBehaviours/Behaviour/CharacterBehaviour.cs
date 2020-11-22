﻿using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    private bool driving;
    private bool playerControlled;

    public bool IsDriving()
    {
        return driving;
    }

    public void SetDriving(bool driving)
    {
        this.driving = driving;
    }

    public bool IsPlayerControlled()
    {
        return playerControlled;
    }

    public void SetPlayerControlled(bool playerControlled)
    {
        this.playerControlled = playerControlled;
    }
}
