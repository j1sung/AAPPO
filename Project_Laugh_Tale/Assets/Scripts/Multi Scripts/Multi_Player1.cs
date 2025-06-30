using Photon.Pun;
using UnityEngine;

public class Multi_Player1 : Multi_Player
{
    protected override void Awake() // 기본 상태
    {
        if (photonView.IsMine)
        {
            base.Awake();  // 부모 클래스의 Start() 호출
        }
    }

}