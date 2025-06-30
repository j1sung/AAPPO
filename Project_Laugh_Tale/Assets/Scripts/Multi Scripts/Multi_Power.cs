using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Multi_Power : MonoBehaviourPun
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Player와 충돌했다면 Power 습득 메소드를 실행
        if (other.gameObject.CompareTag("Player_HitBox"))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                other.gameObject.transform.parent.GetComponent<Multi_Player>().photonView.RPC("get_Power", RpcTarget.All);

                Multi_Player player = other.gameObject.transform.parent.GetComponent<Multi_Player>();
                if (player.GetIsGetPower())
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
}
