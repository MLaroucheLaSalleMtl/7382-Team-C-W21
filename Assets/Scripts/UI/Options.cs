using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Options : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    
    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", value);
    }
    
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", value);
    }
    
    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", value);
    }
}
