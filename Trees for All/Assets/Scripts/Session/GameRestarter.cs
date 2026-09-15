using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sogeti.Session
{
    /// <summary>
    /// Starts the game over.
    /// A scene reload is fresh by construction, so no reset list can rot as the
    /// game grows. This type holds the only SceneManager call, so swapping in a
    /// soft reset later touches one file.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameRestarter : MonoBehaviour
    {
        [Tooltip("Raised by the game over panel. Nothing here knows which button asked.")]
        [SerializeField]
        private VoidEvent restartRequested;

        public void Restart()
        {
            Scene active = SceneManager.GetActiveScene();

            // A scene played from the Project window is not in the build list.
            if (active.buildIndex < 0)
            {
                Debug.LogError($"{name}: {active.name} is not in the build settings, so it cannot reload.", this);
                return;
            }

            SceneManager.LoadScene(active.buildIndex);
        }

        // The no argument overload never replays, so registering cannot reload the scene by itself.
        private void OnEnable()
        {
            if (restartRequested != null)
            {
                restartRequested.Register(Restart);
            }
        }

        private void OnDisable()
        {
            if (restartRequested != null)
            {
                restartRequested.Unregister(Restart);
            }
        }
    }
}
