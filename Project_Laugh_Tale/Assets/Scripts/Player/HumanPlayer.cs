using UnityEngine;

public class HumanPlayer : Player
{
    protected override void Awake() // 기본 상태
    {
        base.Awake();  // 부모 클래스의 Start() 호출
    }
   
}
