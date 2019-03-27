using Cinemachine;
using Opsive.ThirdPersonController.Wrappers;
using UnityEngine;

namespace KopliSoft.CameraControl
{
    public class CameraSwitch : MonoBehaviour
    {
        [SerializeField]
        private CinemachineVirtualCamera currentVirtualCamera;

        private CinemachineBrain brain;
        private CameraController controller;

        // Start is called before the first frame update
        void Start()
        {
            brain = GetComponent<CinemachineBrain>();
            controller = GetComponent<CameraController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (brain.ActiveVirtualCamera == null)
            {
                controller.enabled = true;
                currentVirtualCamera.transform.position = transform.position;
                currentVirtualCamera.transform.rotation = transform.rotation;
            }
            else
            {
                controller.enabled = false;
            }
        }
    }
}
