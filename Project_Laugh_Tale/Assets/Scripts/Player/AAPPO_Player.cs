using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AAPPO_Player : Player
{
    public Agent Agent;

    public override bool get_Power()
    {
        //�Ŀ��� �����ʾҴٸ� �Ŀ��� ����
        if (!playerData.isPower)
        {
            Agent.AddReward(0.05f);
            playerData.isPower = true;
            ring.SetActive(true);
            GameManager.instance.discountPower();
            Debug.Log(playerData.isPower + " Power ȹ��!");
            return true;
        }
        else
        {
            return false;
        }
    }
}
