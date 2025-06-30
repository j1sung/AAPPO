using UnityEngine;

public class ChasingState : State
{
    private float attackAngleRange = 20f; // 공격을 시작할 각도 범위 (20도)
    private float stateStartTime; // 상태 시작 시간
    private float timeLimit = 1.0f; // 상태 유지 시간 제한 (2초)

    public ChasingState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Chasing 시작...");
        stateStartTime = Time.time; // 상태 시작 시간 기록

        // 플레이어를 바라보도록 회전
        /*Vector3 directionToPlayer = (_bot.GetPlayerPosition() - _bot.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
        _bot.transform.rotation = targetRotation;*/
    }

    public override void OnStateUpdate()
    {
        // 상태 제한 시간 확인
        if (Time.time - stateStartTime > timeLimit)
        {
            Debug.Log("Chasing 상태 시간 초과, Patrolling으로 전환.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING); // 시간 초과 시 Patrolling으로 전환
            return;
        }

        FSMPlayer Fsmplayer = _bot.GetComponent<FSMPlayer>();
        Player play = _bot.GetPlayer();
        if (play == null)
        {
            Debug.LogError("Player is null in ChasingState!");
            return;
        }

        Vector3 playerPosition = _bot.GetPlayerPosition();
        float distanceToPlayer = Vector3.Distance(_bot.transform.position, playerPosition);
        Vector3 directionToPlayer = (_bot.GetPlayerPosition() - _bot.transform.position).normalized;
        float angleToPlayer = Vector3.Angle(_bot.transform.up, directionToPlayer);
        //Debug.Log("1");
        _bot.Move(playerPosition);

        
        // 상태 전환 조건
        if ((_bot.IsPlayerInDashAngle(play.transform.position) && play.playerData.isPower) && distanceToPlayer > _bot.attackRange)
        {
            //Debug.Log("2");
            _bot.ChangeState(Bot.BotStateEnum.DASHING);
            
        }
        if (distanceToPlayer <= _bot.attackRange || (angleToPlayer <= attackAngleRange && Fsmplayer.playerData.isPower))
        {
            //Debug.Log("3");
            _bot.ChangeState(Bot.BotStateEnum.ATTACKING);

        }
    }

    public override void OnStateEnd()
    {
        Debug.Log("Chasing 종료.");
    }
}
