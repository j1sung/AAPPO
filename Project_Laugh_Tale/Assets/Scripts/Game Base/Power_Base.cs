using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power_Base : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Player�� �浹�ߴٸ� Power ���� �޼ҵ带 ����
        if(other.gameObject.CompareTag("Player_HitBox"))
        {
            if (other.gameObject.transform.parent.GetComponent<Player_Base>().get_Power())
            {
                Destroy(gameObject);
            }    
        }
    }
}
