using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingState : State
{
    private float attackAngleRange = 5f; // 공격 각도 범위
    private FSMPlayer Fsmplayer;

    public AttackingState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Attacking: 시작");
        Fsmplayer = _bot.GetFSM();

        // 첫 번째 실행 시 상태 확인 및 처리
        HandleAttackOrSwitchState();
    }

    public override void OnStateUpdate()
    {
        // 지속적으로 상태 확인 및 전환 처리
        HandleAttackOrSwitchState();
    }

    private void HandleAttackOrSwitchState()
    {
        // 플레이어와의 거리 및 방향 계산
        float distanceToPlayer = Vector3.Distance(_bot.transform.position, _bot.GetPlayerPosition());
        Vector3 directionToPlayer = (_bot.GetPlayerPosition() - _bot.transform.position).normalized;
        float angleToPlayer = Vector3.Angle(_bot.transform.up, directionToPlayer);

        // 플레이어를 바라보도록 회전
        RotateTowardsPlayer(_bot.GetPlayerPosition());

        // 조건 1: FSM이 Power를 가지고 있고, 플레이어가 20도 각도 내에 있는 경우
        if (Fsmplayer.getPower() && angleToPlayer <= attackAngleRange)
        {
            Debug.Log("Condition 1: Power is active and Player is within angle range.");
            Fsmplayer.Attack(); // FSMPlayer의 Attack 메서드 실행
            _bot.ChangeState(Bot.BotStateEnum.CHASING); // 공격 후 추적 상태로 전환
            return;
        }

        // 조건 2: 플레이어가 사거리 안에 있을 경우
        if (distanceToPlayer <= _bot.attackRange)
        {
            Debug.Log("Condition 2: Player is within attack range.");
            Fsmplayer.Attack(); // FSMPlayer의 Attack 메서드 실행
        }
        else
        {
            Debug.Log("Player is out of range or angle. Switching to Chasing state.");
            _bot.ChangeState(Bot.BotStateEnum.CHASING); // 사거리에서 벗어나면 추적 상태로 전환
        }
    }

    private void RotateTowardsPlayer(Vector3 directionToPlayer)
    {
        _bot.Move(directionToPlayer);
    }

    public override void OnStateEnd()
    {
        Debug.Log("Attacking: 종료");
    }
}
