using System;
using UnityEngine;

public class FSMPlayer : Player
{
    public bool getPower()
    {
        return playerData.isPower;
    }

    public Vector3 GetPlayerPosition()
    {
        return transform.position; // �÷��̾��� ���� ��ġ ��ȯ
    }
}
