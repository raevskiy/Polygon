using KopliSoft.SceneControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debriefing : MonoBehaviour
{
    [SerializeField]
    private string thisScene;
    [SerializeField]
    private string nextScene;

    [SerializeField]
    private GameObject[] activeInNextScene;
    [SerializeField]
    private GameObject[] hiddenInNextScene;

    [SerializeField]
    private string flowchartName;
    [SerializeField]
    private Fungus.Flowchart flowchart;
    [SerializeField]
    private string blockName;

    private SceneController sceneController;

    // Start is called before the first frame update
    void Start()
    {
        sceneController = FindObjectOfType<SceneController>();

        if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
        {
            flowchart = GameObject.Find(flowchartName).GetComponent<Fungus.Flowchart>();
        }

        flowchart.ExecuteBlock(blockName);
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
