using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;

    #region PlayerPrefs
    public const string FullscreenPref = "DoFullscreen";

    public const string MasterVolumePref = "MasterVolume";
    public const string MusicVolumePref = "MusicVolume";
    public const string SfxVolumePref = "SfxVolume";
    public const string UiVolumePref = "UiVolume";

    public const string CamScrollSpeedPref = "CamSpeed";
    #endregion

    public bool fullscreen { get; private set; } = false;
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
    }

    public void LoadAudioPrefs()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumePref, volumeVals.PercentOfRange(0)));
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumePref, volumeVals.PercentOfRange(0)));
        SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumePref, volumeVals.PercentOfRange(0)));
        SetUiVolume(PlayerPrefs.GetFloat(UiVolumePref, volumeVals.PercentOfRange(0)));
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
