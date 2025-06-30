using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Multi_P_Attack : MonoBehaviourPun
{

    [SerializeField] private float p_AttackSpeed = 10f; // 발사 속도
    public void MoveP(Vector2 dir)
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = dir * p_AttackSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Map") || collision.gameObject.CompareTag("Player_HitBox"))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(this.gameObject);
                if (collision.gameObject.CompareTag("Player_HitBox"))
                {
                    collision.gameObject.transform.parent.GetComponent<Multi_Player>().photonView.RPC("Hit", RpcTarget.All);
                }
            }
        }
    }
}
