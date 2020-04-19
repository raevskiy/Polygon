using KopliSoft.SceneControl;
using UnityEngine;
using UnityEngine.Playables;

namespace KopliSoft.CameraControl
{
    public class SkipTimeline : MonoBehaviour
    {
        [SerializeField]
        private int offsetFromLastFrame = 0;
        [SerializeField]
        private GameObject activateOnStopped;
        [SerializeField]
        private string[] hiddenCanvasPaths;

        private PlayableDirector playableDirector;
        private StartOptions startOptions;
        private Canvas[] hiddenCanvases;

        private void Start()
        {
            playableDirector = GetComponent<PlayableDirector>();
            startOptions = FindObjectOfType<StartOptions>();
            playableDirector.played += OnPlayableDirectorPlayed;
            playableDirector.stopped += OnPlayableDirectorStopped;
            hiddenCanvases = new Canvas[hiddenCanvasPaths.Length];
            for (int i = 0; i < hiddenCanvasPaths.Length; i++)
            {
                hiddenCanvases[i] = SceneGraphSearch.Find(hiddenCanvasPaths[i]).GetComponent<Canvas>();
            }
        }

        void OnPlayableDirectorPlayed(PlayableDirector aDirector)
        {
            startOptions.inMainMenu = true;
            foreach (Canvas canvas in hiddenCanvases)
            {
                canvas.enabled = false;
            }
        }

        void OnPlayableDirectorStopped(PlayableDirector aDirector)
        {
            startOptions.inMainMenu = false;
            foreach (Canvas canvas in hiddenCanvases)
            {
                canvas.enabled = true;
            }

            playableDirector.played -= OnPlayableDirectorPlayed;
            playableDirector.stopped -= OnPlayableDirectorStopped;
            enabled = false;
            if (activateOnStopped != null)
            {
                activateOnStopped.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (playableDirector.state == PlayState.Playing && Input.GetButtonDown("Cancel"))
            {
                playableDirector.time = playableDirector.playableAsset.duration - offsetFromLastFrame; // set the time to the last frame
                playableDirector.Evaluate(); // evaluates the timeline
                if (offsetFromLastFrame == 0)
                {
                    playableDirector.Stop(); // deletes the instance of the timeline
                }
            }
        }


    }
}
