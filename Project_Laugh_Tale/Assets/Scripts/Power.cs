using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Player�� �浹�ߴٸ� Power ���� �޼ҵ带 ����
        if(other.gameObject.CompareTag("Player_HitBox"))
        {
            if (other.gameObject.transform.parent.GetComponent<Player>().get_Power())
            {
                Destroy(gameObject);
            }    
        }
    }
}
