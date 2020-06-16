using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class SeamlessSceneUnloader : MonoBehaviour
    {
        [SerializeField]
        private SeamlessSceneLoader loader;

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("MainCamera"))
            {
                loader.Unload();
            }
        }
    }
}
