using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Attack_Base : MonoBehaviour
{
    [SerializeField] private float p_AttackSpeed = 10f; // 발사 속도
    public void MoveP(Vector2 dir)
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = dir * p_AttackSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Map") || collision.gameObject.CompareTag("Player_HitBox"))
        {
            Destroy(this.gameObject);

            if (collision.gameObject.CompareTag("Player_HitBox"))
            {
                collision.gameObject.transform.parent.GetComponent<Player_Base>().Hit();
            }
        }
    }


}
