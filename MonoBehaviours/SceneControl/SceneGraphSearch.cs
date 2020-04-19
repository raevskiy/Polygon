using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class SceneGraphSearch : MonoBehaviour
    {
        public static GameObject Find(string objectPath)
        {
            string[] tokens = objectPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string pathToParent = "/" + string.Join("/", tokens, 0, tokens.Length - 1);
            GameObject parentGameObject = GameObject.Find(pathToParent);
            return parentGameObject.transform.Find(tokens[tokens.Length - 1]).gameObject;
        }
    }
}
