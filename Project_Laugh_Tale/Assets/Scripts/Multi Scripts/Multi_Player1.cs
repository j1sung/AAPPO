using Photon.Pun;
using UnityEngine;

public class Multi_Player1 : Multi_Player
{
    protected override void Awake() // �⺻ ����
    {
        if (photonView.IsMine)
        {
            base.Awake();  // �θ� Ŭ������ Start() ȣ��
        }
    }

}