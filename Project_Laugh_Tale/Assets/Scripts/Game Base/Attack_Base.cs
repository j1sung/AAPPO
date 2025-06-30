using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_Base : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player_HitBox"))
        {
            other.gameObject.transform.parent.GetComponent<Player_Base>().Hit();
        }
    }
}
