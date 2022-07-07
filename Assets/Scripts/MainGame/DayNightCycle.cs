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
    public AnimationCurve fogColor;
    
    private Color _skyColor;
    private Color _equatorColor;
    private Color _groundColor;
    
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
        
        // Environment lighting default values
        _skyColor = RenderSettings.ambientSkyColor;
        _equatorColor = RenderSettings.ambientEquatorColor;
        _groundColor = RenderSettings.ambientGroundColor;
    }

    private void Start()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
        isDay = true;
    }

    private void FixedUpdate()
    {
        // increment time
        time += timeRate * Time.fixedDeltaTime;
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
        float sunIntensity1 = sunIntensity.Evaluate(time);
        sun.intensity = sunIntensity1;
        moon.intensity = moonIntensity.Evaluate(time);
        
        // change light colors
        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(BloodMoonManager.Instance.currentBMProgress);
        
        // change environment lighting (gradient)
        RenderSettings.ambientSkyColor = Color.Lerp(Color.black, _skyColor, sunIntensity1);
        RenderSettings.ambientEquatorColor = Color.Lerp(Color.black, _equatorColor, sunIntensity1);
        RenderSettings.ambientGroundColor = Color.Lerp(Color.black, _groundColor, sunIntensity1);
        
        // change fog 
        RenderSettings.fogDensity = fogIntensityMultiplier.Evaluate(time);
        Color color = RenderSettings.fogColor;
        var fogColor1 = fogColor.Evaluate(time);
        color.r = fogColor1;
        color.g = fogColor1;
        color.b = fogColor1;
        RenderSettings.fogColor = color;
    }

    public static void NewDay()
    {
        RoomManager.Instance.UpdateInfoText("It's a new day, go to the sign to vote!");
        
        RoomManager.Instance.ClearTargets();
        RoomManager.Instance.localPlayer.hasVoted = false;
        RoomManager.Instance.localPlayer.SetCountdowns(false);
        RoomManager.Instance.players.ForEach(r => r.isShielded = false);
        
        VoteMenu.Instance.isDay = true;
        VoteMenu.Instance.isFirstDay = false;
        VoteMenu.Instance.UpdateVoteItems();
    }

    public static void NewNight()
    {
        RoomManager.Instance.UpdateInfoText("It's the night, the powers are activated!");
        RoomManager.Instance.votes.Clear();
        RoomManager.Instance.localPlayer.vote = null;
        RoomManager.Instance.localPlayer.SetCountdowns(true);
        RoomManager.Instance.localPlayer.UpdateActionText(Role.AtMessage.Clear);
        
        VoteMenu.Instance.isDay = false;
        VoteMenu.Instance.isFirstDay = false;
        VoteMenu.Instance.UpdateVoteItems();
        
        QuestManager.Instance.AssignQuests();
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
