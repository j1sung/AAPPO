using Photon.Pun;
using UnityEngine;

public class Multi_Player2 : Multi_Player
{
    protected override void Awake()
    {
        if (photonView.IsMine)
        {
            base.Awake();  // �θ� Ŭ������ Start() ȣ��
        }
    }
}
