using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Vector3 playerPosition;
    public Quaternion playerRotation;

    public Vector3 airshipPosition;
    public Quaternion airshipRotation;

    public Dictionary<string, StorageItemData[]> storageItems;

    public float timeOfDay;

    public GameData()
    {
        playerPosition = new Vector3(870.689f, 63.777f, 819.854f);
        playerRotation = Quaternion.Euler(0, -163.377f, 0);

        airshipPosition = new Vector3(878.299988f, 75.5f, 840f);
        airshipRotation = Quaternion.Euler(0, 200f, 0);

        storageItems = new Dictionary<string, StorageItemData[]>();

        timeOfDay = 18;
    }
}
