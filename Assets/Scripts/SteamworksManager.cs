using MarTools;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)]
public class SteamworksManager : Singleton<SteamworksManager>
{
    public uint appID;
    protected override void Initialize()
    {
        base.Initialize();
        try
        {
            Steamworks.SteamClient.Init(appID);
        }
        catch (System.Exception e)
        {
            // Something went wrong - it's one of these:
            //
            //     Steam is closed?
            //     Can't find steam_api dll?
            //     Don't have permission to play app?
            //

            UnityEngine.Debug.LogWarning(e.Message);
        }

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SceneManager.LoadScene(1);
        }

    }

    private void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }
}
