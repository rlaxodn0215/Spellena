using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCenterDataType;

public class OccupationArea : MonoBehaviour
{
    private Dictionary<string, GameObject> playerRed = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> playerBlue = new Dictionary<string, GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        AddPlayer(other);
    }

    private void OnTriggerExit(Collider other)
    {
        RemovePlayer(other);
    }

    private void AddPlayer(Collider other)
    {
        if (other.GetComponent<PlayerCommon>())
        {
            if (other.gameObject.tag == "TeamA")
            {
                playerRed.Add(other.name, other.gameObject);
            }

            else if (other.gameObject.tag == "TeamB")
            {
                playerBlue.Add(other.name, other.gameObject);
            }
        }
    }

    private void RemovePlayer(Collider other)
    {
        if (other.GetComponent<PlayerCommon>())
        {
            if (other.gameObject.tag == "TeamA")
            {
                playerRed.Remove(other.name);
            }

            else if (other.gameObject.tag == "TeamB")
            {
                playerBlue.Remove(other.name);
            }
        }
    }

    public int GetTeamCount(string team)
    {
        if(team == "A")
        {
            return playerRed.Count;
        }

        else
        {
            return playerBlue.Count;
        }
    }
}
