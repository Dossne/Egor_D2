using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockyHoles
{
    /// <summary>
    /// Redirects from placeholder scenes (for example SampleScene) to Home,
    /// so Play starts with the actual gameplay flow.
    /// </summary>
    public static class BootstrapSceneLoader
    {
        private const string GameplayStartScene = "Home";
        private const string PlaceholderScene = "SampleScene";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LoadGameplaySceneFromPlaceholder()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != PlaceholderScene)
            {
                return;
            }

            SceneManager.LoadScene(GameplayStartScene);
        }
    }
}
