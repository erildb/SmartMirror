using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CognitiveServicesTTS;
using System;

public class UIManager : MonoBehaviour
{

    public SpeechManager speech;
    public InputField input;
    // public InputField pitch;
    //public Toggle useSDK;
    // public Dropdown voicelist;

    private void Start()
    {
        //  string pitch = "0";

        List<string> voices = new List<string>();
        foreach (VoiceName voice in Enum.GetValues(typeof(VoiceName)))
        {
            voices.Add(voice.ToString());
        }
        //  voicelist.AddOptions(voices);
        // voicelist.value = (int)VoiceName.enUSJessaRUS;
    }

    public void SpeechPlayback()
    {
        if (speech.isReady)
        {
            string msg = ""; //"Welcome to smart mirror. Do you want to take a picture?";
            speech.voiceName = (VoiceName)(int)VoiceName.enUSJessaRUS;
            speech.VoicePitch = 0;
            speech.SpeakWithSDKPlugin(msg);
            //speech.SpeakWithRESTAPI(msg);
        }
        else
        {
            //Debug.Log("SpeechManager is not ready. Wait until authentication has completed.");
        }
    }

    public void ClearText()
    {
        input.text = "";
    }
}
