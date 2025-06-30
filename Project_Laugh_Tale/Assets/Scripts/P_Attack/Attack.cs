using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player_HitBox"))
        {
            other.gameObject.transform.parent.GetComponent<Player>().Hit();
        }
    }
}
