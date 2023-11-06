using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectPool : MonoBehaviour
{
    public int poolSize;
    private List<GameObject> playerItemPool;

    // Start is called before the first frame update
    public void Init()
    {
        poolSize = PhotonNetwork.CurrentRoom.PlayerCount;
        playerItemPool = new List<GameObject>();
        
        for(int i = 0; i < poolSize; i++)
        {
            GameObject _obj = PhotonNetwork.Instantiate("PlayerItem", Vector3.zero, Quaternion.identity);
            _obj.SetActive(false);
            playerItemPool.Add(_obj);
        }
    }

    public List<GameObject> GetPlayerItemPool()
    {
        int _playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        List<GameObject> activePlayers = new List<GameObject>();
        foreach (var _item in playerItemPool)
        {
            if(!_item.activeInHierarchy)
            {
                _item.SetActive(true);
                activePlayers.Add(_item);
                if (activePlayers.Count >= _playerCount)
                {
                    break;
                }
            }
        }
        return activePlayers;
    }

    public void ReturnPlayerItemPool(PlayerItem _item)
    {
        _item.gameObject.SetActive(false);
    }
}
