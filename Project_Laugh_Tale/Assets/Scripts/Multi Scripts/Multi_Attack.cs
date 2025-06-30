using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi_Attack : MonoBehaviourPun
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player_HitBox"))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                other.gameObject.transform.parent.GetComponent<Multi_Player>().photonView.RPC("Hit", RpcTarget.All);
            }
        }
    }
}