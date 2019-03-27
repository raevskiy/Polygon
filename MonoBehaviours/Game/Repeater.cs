using System;
using UnityEngine;

public class Repeater : MonoBehaviour
{
    [SerializeField]
    private string pathToLeaderObject;
    private GameObject leaderObject;
    [SerializeField]
    private GameObject followerObject;

    void Start()
    {
        leaderObject = DoFind(pathToLeaderObject);
    }

    void Update()
    {
        followerObject.transform.position = leaderObject.transform.position;
        followerObject.transform.rotation = leaderObject.transform.rotation;
    }

    private GameObject DoFind(string objectPath)
    {
        string[] tokens = objectPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string pathToParent = "/" + string.Join("/", tokens, 0, tokens.Length - 1);
        GameObject parentGameObject = GameObject.Find(pathToParent);
        return parentGameObject.transform.Find(tokens[tokens.Length - 1]).gameObject;
    }

}
