using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BGMManager : MonoBehaviourPunCallbacks
{
    [HideInInspector]
    public AudioSource audioSource;

    public List<AudioData> tempDatas = new List<AudioData>();
    private Dictionary<string, AudioClip> audioDatas = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct AudioData
    {
        public string name;
        public AudioClip clip;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (photonView != null && photonView.ViewID != 0)
        {
            photonView.ViewID = 0;
            PhotonNetwork.RegisterPhotonView(photonView);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        LinkDatas();

        PlayLoadingCharacterBGM();
    }

    void LinkDatas()
    {
        foreach(AudioData data in tempDatas)
        {
            audioDatas.Add(data.name, data.clip);
        }
    }

    public void VolumeControl(float vol)
    {
        if (audioSource.clip == null) return;
    }


    public void PlayLoadingCharacterBGM()
    {
        if(audioDatas.ContainsKey("LoadingCharacter"))
            audioSource.clip = audioDatas["LoadingCharacter"];
        audioSource.Play();
        audioSource.loop = true;
    }

    public void PlayDuringRoundBGM()
    {
        if (audioDatas.ContainsKey("DuringRound"))
            audioSource.clip = audioDatas["DuringRound"];
        audioSource.Play();
        audioSource.loop = true;
    }



}
