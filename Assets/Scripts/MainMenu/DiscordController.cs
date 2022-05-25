using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;

public class DiscordController : MonoBehaviour
{
    public Discord.Discord discord;
    void Start()
    {
        try
        {
            discord = new Discord.Discord(976739443060932638, (System.UInt64) Discord.CreateFlags.Default);
            var activityManager = discord.GetActivityManager();
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
                if (res == Discord.Result.Ok)
                    Debug.Log("Discord activity has been set up");
                else
                    Debug.Log("Discord activity could not be found !");
            });
        }

        catch
        {
            Debug.Log("Discord not launched on game start !");
        }
    }

    void Update()
    {
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            // Avoiding game crash if Discord not launched
        }
    }
}
