using UnityEngine;

public class DiscordController : MonoBehaviour
{
    public Discord.Discord Discord;
    private bool _discordRunning = false;
    void StartRichPresence()
    {
        Discord = new Discord.Discord(976739443060932638, (System.UInt64) global::Discord.CreateFlags.Default);
            var activityManager = Discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = "DEBATE - HIDE - SURVIVE",
                State = "Play for free at www.lycans.games",
                Assets =
                {
                    LargeImage = "v7"
                }
            };
            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res == global::Discord.Result.Ok)
                    Debug.Log("Discord activity has been set up");
                else
                    Debug.Log("Discord activity could not be found !");
            });
    }

    // Checking if Discord is installed and launched
    private void Start()
    {
        for (int i = 0; i < System.Diagnostics.Process.GetProcesses().Length; i++)
        {
            if (System.Diagnostics.Process.GetProcesses()[i].ToString() == "System.Diagnostics.Process (Discord)")
            {
                _discordRunning = true;
                StartRichPresence();
                break;
            }
        }
    }

    void Update()
    {
        if (_discordRunning)
            Discord.RunCallbacks();
    }
}
