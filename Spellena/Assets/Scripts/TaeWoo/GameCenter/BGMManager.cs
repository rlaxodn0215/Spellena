using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BGMManager : MonoBehaviourPunCallbacks
{
    public static BGMManager instance;

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

        if (photonView != null && photonView.ViewID !=0)
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

        PlayBGM("LoadingCharacter",1.0f, true);
    }

    void LinkDatas()
    {
        foreach(AudioData data in tempDatas)
        {
            audioDatas.Add(data.name, data.clip);
        }
    }

    [PunRPC]
    public void VolumeControl(float size, bool isIncrease)
    {
        if (audioSource.clip == null) return;

        if(isIncrease)
        {
            audioSource.volume += size;
        }

        else
        {
            audioSource.volume -= size;
        }
    }

    [PunRPC]
    public void PlayBGM(string key, float vol, bool isLoop)
    {
        if (audioDatas.ContainsKey(key))
            audioSource.clip = audioDatas[key];

        audioSource.volume = vol;
        audioSource.Play();
        audioSource.loop = isLoop;
    }

}
