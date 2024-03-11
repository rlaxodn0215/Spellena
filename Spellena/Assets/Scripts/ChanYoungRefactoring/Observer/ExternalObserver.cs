using ObserverData;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalObserver : MonoBehaviourPunCallbacks, IPunObservable
{

    private ObserverFrame<Vector3> externalForceObserver;

    private void Start()
    {
        externalForceObserver = new ObserverFrame<Vector3>();
    }

    public void NotifyAddExternalForce(Vector3 externalForce)
    {
        photonView.RPC("AddExternalForce", photonView.Owner, externalForce);
    }

    public void RaiseFunction(FunctionFrame<Vector3> frame)
    {
        externalForceObserver.RaiseObserver(frame);
    }

    public void LowerFunction(FunctionFrame<Vector3> frame)
    {
        externalForceObserver.LowerObserver(frame);
    }

    [PunRPC]
    public void AddExternalForce(Vector3 externalForce)
    {
        externalForceObserver.NotifyChanged(externalForce);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
