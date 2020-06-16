using KopliSoft.SceneControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDebriefing : MonoBehaviour
{
    [SerializeField]
    private string thisScene;
    [SerializeField]
    private string nextScene;

    [SerializeField]
    private GameObject[] activeInNextScene;
    [SerializeField]
    private GameObject[] hiddenInNextScene;

    private SceneController sceneController;

    // Start is called before the first frame update
    void Start()
    {
        sceneController = FindObjectOfType<SceneController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            sceneController.FadeAndSwitchScenes(nextScene, thisScene);
        }
    }
}
