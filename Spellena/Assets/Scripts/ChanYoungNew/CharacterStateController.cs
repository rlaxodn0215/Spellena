using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEditorInternal;
using System;

public class CharacterStateController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Action<string, float> PlayScreenEffect;

    public enum BuffState
    {
        None, Horror
    }

    List<BuffState> state = new List<BuffState>();
    List<float> leftTime = new List<float>();

    void Start()
    {
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            for (int i = 0; i < leftTime.Count; i++)
            {
                leftTime[i] -= Time.fixedDeltaTime;
            }

            for (int i = 0; i < leftTime.Count; i++)
            {
                if (leftTime[i] <= 0f)
                {
                    leftTime.RemoveAt(i);
                    state.RemoveAt(i);
                    i = -1;
                }
            }

            PlayState();
        }
    }

    public void AddState(string stateName, float stateTime)
    {
        BuffState _tempState = BuffState.None;
        if (stateName == "Horror")
        {
            _tempState = BuffState.Horror;
        }

        if(_tempState != BuffState.None)
        {
            int _checkState = CheckState(_tempState);
            if (_checkState >= 0)
            {
                if(leftTime[_checkState] < stateTime)
                    leftTime[_checkState] = stateTime;
            }
            else
            {
                state.Add(_tempState);
                leftTime.Add(stateTime);
            }
        }
    }

    private void PlayState()
    {
        for(int i = 0; i < state.Count; i++)
        {
            if (state[i] == BuffState.Horror)
                PlayHorror();

        }
    }

    private void PlayHorror()
    {
        //공포 효과 -> 컬티스트를 바라봄
    }

    private int CheckState(BuffState buffState)
    {
        for(int i = 0; i < state.Count; i++)
        {
            if (state[i] == buffState)
                return i;
        }
        return -1;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
