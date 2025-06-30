using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Player와 충돌했다면 Power 습득 메소드를 실행
        if(other.gameObject.CompareTag("Player_HitBox"))
        {
            if (other.gameObject.transform.parent.GetComponent<Player>().get_Power())
            {
                Destroy(gameObject);
            }    
        }
    }
}
