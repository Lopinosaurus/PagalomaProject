using System.Collections.Generic;
using MainGame;
using MainGame.Helpers;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using UnityEngine;
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance;
    
    [Range(0.0f, 1.0f)]
    public float time;
    public float fullDayLength; // Day length in seconds
    public float startTime = 0.4f;
    [Range(0, 0.05f)] [SerializeField] private float timeRate;
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

    [Header("Fog")]
    public AnimationCurve fogIntensityMultiplier;
    
    private PhotonView _pv;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        
        _pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
        isDay = true;
    }

    private void Update()
    {
        // TODO Change day night cycle to make lights static ?
        
        // increment time
        time += timeRate * Time.deltaTime;
        time = time >= 1 ? 0 : time;

        if (time > 0.25 && time < 0.75) // New day
        {
            if (isDay == false)
            {
                isDay = true;
                if (PhotonNetwork.IsMasterClient)
                {
                    _pv.RPC(nameof(RPC_NewDay), RpcTarget.All, time);
                
                    int isEog = RoomManager.Instance.CheckIfEog();
                    if (isEog != 0) _pv.RPC(nameof(RPC_EOG), RpcTarget.All, isEog);
                }
            }
        } else if (isDay) // New night
        {
            isDay = false;
            if (PhotonNetwork.IsMasterClient)
            {
                if (VoteMenu.Instance.isFirstDay == false) RoomManager.Instance.ResolveVote();
                
                int isEog = RoomManager.Instance.CheckIfEog();
                if (isEog != 0) _pv.RPC(nameof(RPC_EOG), RpcTarget.All, isEog);
                
                _pv.RPC(nameof(RPC_NewNight), RpcTarget.All, time);
            }
        }
        
        // light rotation
        sun.transform.eulerAngles = noon * ((time - 0.25f) * 4.0f);
        moon.transform.eulerAngles = noon * ((time - 0.75f) * 4.0f);
        
        // light intensity
        sun.intensity = sunIntensity.Evaluate(time);
        moon.intensity = moonIntensity.Evaluate(time);
        
        // change colors
        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(time);
        
        // DON'T MODIFY ACTIVE STATES IN AN UPDATE LOOP !!!
        
        // // enable / disable the sun
        // if (sun.intensity == 0 && sun.gameObject.activeInHierarchy)
        //     sun.gameObject.SetActive(false);
        // else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
        //     sun.gameObject.SetActive(true);
        //
        // // enable / disable the moon
        // if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
        //     moon.gameObject.SetActive(false);
        // else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
        //     moon.gameObject.SetActive(true);
        
        // lighting and reflections intensity
        // RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        // RenderSettings.reflectionIntensity = reflectionsIntensityMultiplier.Evaluate(time);

        // fog intensity
        RenderSettings.fogDensity = fogIntensityMultiplier.Evaluate(time);
    }

    public static void NewDay()
    {
        RoomManager.Instance.UpdateInfoText("It's a new day, go to the sign to vote!");
        VoteMenu.Instance.isDay = true;
        VoteMenu.Instance.isFirstDay = false;
        RoomManager.Instance.localPlayer.hasVoted = false;
        RoomManager.Instance.localPlayer.SetCountdowns(false);
        RoomManager.Instance.ClearTargets();
        VoteMenu.Instance.UpdateVoteItems();
    }

    public static void NewNight()
    {
        RoomManager.Instance.UpdateInfoText("It's the night, the powers are activated!");
        RoomManager.Instance.votes = new List<Role>();
        RoomManager.Instance.localPlayer.vote = null;

        RoomManager.Instance.localPlayer.SetCountdowns(true);
        RoomManager.Instance.localPlayer.UpdateActionText();
        
        VoteMenu.Instance.isDay = false;
        VoteMenu.Instance.isFirstDay = false;
        VoteMenu.Instance.UpdateVoteItems();
    }

    [PunRPC]
    private void RPC_NewDay(float time)
    {
        this.time = time;
        isDay = true;
        NewDay();
    }
    
    [PunRPC]
    private void RPC_NewNight(float time)
    {
        this.time = time;
        isDay = false;
        NewNight();
    }

    [PunRPC]
    private void RPC_EOG(int isEog) => RoomManager.Instance.DisplayEndScreen(isEog);
}
