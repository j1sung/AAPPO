using UnityEngine;

public class EnemyPlayer_Base : Player_Base
{
    protected override void Awake()
    {
        base.Awake(); // 부모 클래스의 Start() 호출
    }
   
}
