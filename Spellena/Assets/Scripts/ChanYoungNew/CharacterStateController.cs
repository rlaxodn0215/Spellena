using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
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
    List<PhotonView> targetView = new List<PhotonView> ();

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
                    targetView.RemoveAt(i);
                    i = -1;
                }
            }

            PlayState();
        }
    }

    public void AddState(string stateName, float stateTime, int sender)
    {
        PhotonView _sendView = PhotonNetwork.GetPhotonView(sender);
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
                if (leftTime[_checkState] < stateTime)
                {
                    leftTime[_checkState] = stateTime;
                    targetView[_checkState] = _sendView;
                }
            }
            else
            {
                state.Add(_tempState);
                leftTime.Add(stateTime);
                targetView.Add(_sendView);
            }
        }
    }

    private void PlayState()
    {
        for(int i = 0; i < state.Count; i++)
        {
            if (state[i] == BuffState.Horror)
                PlayHorror(i);

        }
    }

    private void PlayHorror(int index)
    {
        Vector3 _direction = targetView[index].transform.position - transform.position;
        _direction = new Vector3(0, _direction.y, 0);
        transform.LookAt(targetView[index].transform.position + new Vector3(0, 1, 0));
        Vector3 _euler = transform.rotation.eulerAngles;

        transform.rotation = Quaternion.Euler(0, _euler.y, 0);

        float _angle = GlobalOperation.Instance.NormalizeAngle(_euler.x);
        if (_angle > 60)
            _angle = 60;
        else if (_angle < -60)
            _angle = -60;

        GetComponent<PlayerCommon>().cameraMain.transform.rotation = Quaternion.Euler(_angle, 0, 0);
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
