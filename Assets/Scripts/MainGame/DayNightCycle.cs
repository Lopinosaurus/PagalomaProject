using System;
using System.Collections;
using System.Collections.Generic;
using MainGame;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float time;
    public float fullDayLength; // Day length in seconds
    public float startTime = 0.4f;
    private float timeRate;
    public Vector3 noon;
    public bool isDay;
    
    [Header("Sun")]
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;
    
    [Header("Moon")]
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    [Header("Other Lighting")]
    public AnimationCurve lightingIntensityMultiplier;
    public AnimationCurve reflectionsIntensityMultiplier;
    public AnimationCurve fogIntensityMultiplier;
    
    [SerializeField] private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
        isDay = true;
    }

    private void Update()
    {
        // increment time
        time += timeRate * Time.deltaTime;
        if (time >= 1.0f) time = 0.0f;

        if (time > 0.25 && time < 0.75 && isDay == false) // New day
        {
            isDay = true;
            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("RPC_NewDay", RpcTarget.Others, time);
                NewDay();
                
                int isEOG = RoomManager.Instance.CheckIfEOG();
                if (isEOG != 0) PV.RPC("RPC_EOG", RpcTarget.All, isEOG);
            }
        } else if ((time <= 0.25 || time >= 0.75) && isDay) // New night
        {
            isDay = false;
            if (PhotonNetwork.IsMasterClient)
            {
                if (VoteMenu.Instance.isFirstDay == false) RoomManager.Instance.ResolveVote();
                
                int isEOG = RoomManager.Instance.CheckIfEOG();
                if (isEOG != 0) PV.RPC("RPC_EOG", RpcTarget.All, isEOG);
                
                PV.RPC("RPC_NewNight", RpcTarget.Others, time);
                NewNight();
            }
        }
        
        // light rotation
        sun.transform.eulerAngles = (time - 0.25f) * noon * 4.0f;
        moon.transform.eulerAngles = (time - 0.75f) * noon * 4.0f;
        
        // light intensity
        sun.intensity = sunIntensity.Evaluate(time);
        moon.intensity = moonIntensity.Evaluate(time);
        
        // change colors
        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(time);
        
        // enable / disable the sun
        if (sun.intensity == 0 && sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(false);
        else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(true);
        
        // enable / disable the moon
        if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(false);
        else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(true);
        
        // lighting and reflections intensity
        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionsIntensityMultiplier.Evaluate(time);

        // fog intensity
        RenderSettings.fogDensity = fogIntensityMultiplier.Evaluate(time);
    }
    
    public void NewDay()
    {
        RoomManager.Instance.UpdateInfoText("It's a new day, go to the sign to vote!");
        VoteMenu.Instance.isDay = true;
        VoteMenu.Instance.isFirstDay = false;
        RoomManager.Instance.localPlayer.hasVoted = false;
        RoomManager.Instance.localPlayer.hasCooldown = true;
        RoomManager.Instance.ClearTargets();
        VoteMenu.Instance.UpdateVoteItems();
    }

    public void NewNight()
    {
        RoomManager.Instance.UpdateInfoText("It's the night, the powers are activated!");
        RoomManager.Instance.votes = new List<Role>();
        VoteMenu.Instance.isDay = false;
        VoteMenu.Instance.isFirstDay = false;
        RoomManager.Instance.localPlayer.hasCooldown = false;
        RoomManager.Instance.localPlayer.UpdateActionText();
        RoomManager.Instance.localPlayer.vote = null;
        VoteMenu.Instance.UpdateVoteItems();
    }

    [PunRPC]
    void RPC_NewDay(float time)
    {
        this.time = time;
        isDay = true;
        NewDay();
    }
    
    [PunRPC]
    void RPC_NewNight(float time)
    {
        this.time = time;
        isDay = false;
        NewNight();
    }

    [PunRPC]
    void RPC_EOG(int isEOG)
    {
        RoomManager.Instance.DisplayEndScreen(isEOG);
    }
}
