using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Use the PointerDown and PointerUP interfaces to detect a mouse down and up on a ui element
public class AudioRecorder : MonoBehaviour
{
    AudioClip recordedAudioClip;

    //Keep this one as a global variable (outside the functions) too and use GetComponent during start to save resources
    AudioSource audioSource;
    private float startRecordingTime;

    private bool isRecording = false;

    public Text recordButtonText;

    //Get the audiosource here to save resources
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnButtonRecord()
    {
        if (!isRecording)
        {
            StartRecord();
            isRecording = true;
            recordButtonText.text = "Stop Record";
        }
        else
        {
            StopRecord();
            isRecording = false;
            recordButtonText.text = "Start Record";
        }
    }

    public void StopRecord()
    {
        //End the recording when the mouse comes back up, then play it
        Microphone.End("");

        //Trim the audioclip by the length of the recording
        AudioClip recordingNew = AudioClip.Create(recordedAudioClip.name,
            (int) ((Time.time - startRecordingTime) * recordedAudioClip.frequency), recordedAudioClip.channels,
            recordedAudioClip.frequency, false);
        float[] data = new float[(int) ((Time.time - startRecordingTime) * recordedAudioClip.frequency)];
        recordedAudioClip.GetData(data, 0);
        recordingNew.SetData(data, 0);
        this.recordedAudioClip = recordingNew;

        //Play recording
        audioSource.clip = recordedAudioClip;
        audioSource.Play();
    }

    public void StartRecord()
    {
        //Get the max frequency of a microphone, if it's less than 44100 record at the max frequency, else record at 44100
        int minFreq;
        int maxFreq;
        int freq = 44100;
        Microphone.GetDeviceCaps("", out minFreq, out maxFreq);
        if (maxFreq < 44100)
            freq = maxFreq;

        //Start the recording, the length of 300 gives it a cap of 5 minutes
        recordedAudioClip = Microphone.Start("", false, 300, 44100);
        startRecordingTime = Time.time;
    }
}