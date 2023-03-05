using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPersistanceManager : MonoBehaviour
{
    public static DataPersistanceManager instance { get; private set; }

    private GameData gameData;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found multiple persistence managers in the scene");
        }
        instance = this;
    }

    // Update is called once per frame
    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadGame()
    {
        if (gameData == null)
        {
            Debug.Log("No data found. Starting a new game");
            NewGame();
        }
    }

    public void SaveGame()
    {

    }
}
