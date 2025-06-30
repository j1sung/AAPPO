using UnityEngine;

public class ItemState : State
{
    private float stateStartTime; // 상태 시작 시간
    private float timeLimit = 1.0f; // 상태 유지 시간 제한 (2초)

    public ItemState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Item 상태 시작...");
        stateStartTime = Time.time; // 상태 시작 시간 기록
    }

    public override void OnStateUpdate()
    {
        // 상태 제한 시간 확인
        if (Time.time - stateStartTime > timeLimit)
        {
            Debug.Log("Item 상태 시간 초과, Patrolling으로 전환.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING); // 시간 초과 시 Patrolling으로 전환
            return;
        }

        Vector3 powerPosition = _bot.targetPosition;
        if (powerPosition == null)
        {
            Debug.LogWarning("Power Position is null.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING); // 위치 정보가 없으면 Patrolling으로 전환
            return;
        }

        // 목표 방향 계산
        Vector3 directionToPower = (powerPosition - _bot.transform.position).normalized;

        // 매끄러운 회전
        //Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPower);
        //_bot.transform.rotation = Quaternion.Slerp(_bot.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 목표 위치로 이동
        _bot.Move(powerPosition);

        // 목표에 도달하면 상태 변경
        if (Vector3.Distance(_bot.transform.position, powerPosition) < 0.1f)
        {
            Debug.Log("Power에 도달, Patrolling으로 전환.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING);
        }
    }

    public override void OnStateEnd()
    {
        Debug.Log("Item 상태 종료.");
    }
}
