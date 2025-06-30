using Photon.Pun;
using UnityEngine;

public class Multi_Player2 : Multi_Player
{
    protected override void Awake()
    {
        if (photonView.IsMine)
        {
            base.Awake();  // 부모 클래스의 Start() 호출
        }
    }
}
