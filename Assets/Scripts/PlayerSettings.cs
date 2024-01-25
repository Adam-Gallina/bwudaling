using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;

    #region PlayerPrefs
    public const string FullscreenPref = "DoFullscreen";
    public const string CamScrollSpeedPref = "CamSpeed";
    public const string ShowSpeedrunTimerPref = "SpeedrunTimer";
    public const string NicknamePref = "Nickname";

    public const string MasterVolumePref = "MasterVolume";
    public const string MusicVolumePref = "MusicVolume";
    public const string SfxVolumePref = "SfxVolume";
    public const string UiVolumePref = "UiVolume";
    #endregion

    public bool fullscreen { get; private set; } = false;
    public string nickname { get; private set; } = "";
    public string RealNickname { 
        get 
        {
            if (nickname != "")
                return nickname;
            else
                return ManagerDebug.Instance.DEBUG_useKcpManager ? "Unnamed KCP Player" : SteamFriends.GetPersonaName();
        } 
    }
    public bool speedrunTimer { get; private set; } = false;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private RangeF volumeVals;
    public float masterVolume { get { mixer.GetFloat(MasterVolumePref, out float o); return volumeVals.PercentOfRange(o); } }
    public float musicVolume { get { mixer.GetFloat(MusicVolumePref, out float o); return volumeVals.PercentOfRange(o); } }
    public float sfxVolume { get { mixer.GetFloat(SfxVolumePref, out float o); return volumeVals.PercentOfRange(o); } }
    public float uiVolume { get { mixer.GetFloat(UiVolumePref, out float o); return volumeVals.PercentOfRange(o); } }


    [SerializeField] private RangeF camScrollSpeedRange;
    public float camScrollVal { get; private set; }
    public float CamScrollSpeed { get { return camScrollSpeedRange.PercentVal(camScrollVal); } }

    private void Awake()
    {
        Instance = this;
        LoadPrefs();
    }

    public void LoadPrefs()
    {
        fullscreen = PlayerPrefs.GetInt(FullscreenPref, 0) == 1;
        SetFullscreen(fullscreen);

        camScrollVal = PlayerPrefs.GetFloat(CamScrollSpeedPref, camScrollSpeedRange.PercentOfRange(15));
        SetCamScrollSpeed(camScrollVal);

        speedrunTimer = PlayerPrefs.GetInt(ShowSpeedrunTimerPref, 0) == 1;
        SetSpeedrunTimer(speedrunTimer);
    }

    public void LoadLatePrefs()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumePref, volumeVals.PercentOfRange(0)));
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumePref, volumeVals.PercentOfRange(0)));
        SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumePref, volumeVals.PercentOfRange(0)));
        SetUiVolume(PlayerPrefs.GetFloat(UiVolumePref, volumeVals.PercentOfRange(0)));

        nickname = PlayerPrefs.GetString(NicknamePref, "");
        SetNickname(nickname);
    }

    public void SetFullscreen(bool fullscreen)
    {
        if (fullscreen != this.fullscreen)
        {
            PlayerPrefs.SetInt(FullscreenPref, fullscreen ? 1 : 0);
            this.fullscreen = fullscreen;
        }

        if (fullscreen)
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreen);
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    public void SetSpeedrunTimer(bool show)
    {
        if (show != speedrunTimer)
        {
            speedrunTimer = show;
            PlayerPrefs.SetInt(ShowSpeedrunTimerPref, show ? 1 : 0);
        }
    }

    public void SetNickname(string nickname)
    {
        if (nickname != this.nickname)
        {
            PlayerPrefs.SetString(NicknamePref, nickname);
            this.nickname = nickname;
            if (BwudalingNetworkManager.Instance.ActivePlayer)
                BwudalingNetworkManager.Instance.ActivePlayer.CmdSetDisplayName(RealNickname);
        }
    }

    public void SetMasterVolume(float volume)
    {
        float val = volume != 0 ? volumeVals.PercentVal(volume) : -80;
        if (masterVolume != val)
            PlayerPrefs.SetFloat(MasterVolumePref, volume);

        mixer.SetFloat(MasterVolumePref, val);
    }
    public void SetMusicVolume(float volume)
    {
        float val = volume != 0 ? volumeVals.PercentVal(volume) : -80;
        if (musicVolume != val)
            PlayerPrefs.SetFloat(MusicVolumePref, volume);

        mixer.SetFloat(MusicVolumePref, val);
    }
    public void SetSfxVolume(float volume)
    {
        float val = volume != 0 ? volumeVals.PercentVal(volume) : -80;
        if (sfxVolume != val)
            PlayerPrefs.SetFloat(SfxVolumePref, volume);

        mixer.SetFloat(SfxVolumePref, val);
    }
    public void SetUiVolume(float volume)
    {
        float val = volume != 0 ? volumeVals.PercentVal(volume) : -80;
        if (uiVolume != val)
            PlayerPrefs.SetFloat(UiVolumePref, volume);

        mixer.SetFloat(UiVolumePref, val);
    }

    public void SetCamScrollSpeed(float t)
    {
        if (camScrollVal != t)
            PlayerPrefs.SetFloat(CamScrollSpeedPref, t);

        camScrollVal = t;
    }
}
