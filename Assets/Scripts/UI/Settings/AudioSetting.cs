using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    public Slider MainSlider;
    public Slider BGMSlider;
    public Slider SFXSlider;


    public AudioMixer MainMixer;

    //key×Ö·û´®
    private string MainKey = "MainVol";
    private string BGMKey = "BGMVol";
    private string SFXKey = "SFXVol";

    // Start is called before the first frame update
    void Start()
    {
        MainSlider.value = PlayerPrefs.GetFloat(MainKey, 0.75f);
        BGMSlider.value = PlayerPrefs.GetFloat(BGMKey, 0.75f);
        SFXSlider.value = PlayerPrefs.GetFloat(SFXKey, 0.75f);

        SetMixerVolume("MasterVol", MainSlider.value);
        SetMixerVolume("BGMVol", BGMSlider.value);
        SetMixerVolume("SFXVol", SFXSlider.value);

        MainSlider.onValueChanged.AddListener(value => OnAudioValueChanged("MasterVol", MainKey, value));
        BGMSlider.onValueChanged.AddListener(value => OnAudioValueChanged("BGMVol", MainKey, value));
        SFXSlider.onValueChanged.AddListener(value => OnAudioValueChanged("SFXVol", MainKey, value));
    }

    //»¬¿é±»ÍÏ¶¯Ê±
    public void OnAudioValueChanged(string MixerName,string KeyName,float value)
    {
        SetMixerVolume(MixerName, value);
        PlayerPrefs.SetFloat(KeyName, value);
        PlayerPrefs.Save();
    }

    public void SetMixerVolume(string name,float number)//Mixer·¶Î§-80-0db
    {
        float volvalue = Mathf.Log10(number) * 20f;
        MainMixer.SetFloat(name, volvalue);
    }
}
