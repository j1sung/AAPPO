using System.Collections;
using UnityEngine;

public class DashState : State
{
    private FSMPlayer Fsmplayer;

    public DashState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("===== Start Dash =====");
        Fsmplayer = _bot.GetFSM();

        // 처음 실행 시 대시 조건 확인 및 처리
        HandleDashOrSwitchState();
    }

    public override void OnStateUpdate()
    {
        // 대시 조건을 지속적으로 확인
        HandleDashOrSwitchState();
    }

    private void HandleDashOrSwitchState()
    {
        if (Fsmplayer.getPower()) // Power를 가지고 있는 경우
        {
            Vector3 playerPosition = _bot.GetPlayerPosition();

            // 플레이어가 바라보는 방향과 Bot의 상대 방향 계산
            Vector3 playerForward = _bot.GetPlayerup(); // 플레이어가 바라보는 방향
            Vector3 botDirection = _bot.transform.position - playerPosition; // Bot 위치에서 플레이어까지의 벡터
            // 플레이어가 보는 방향과 Bot의 방향 간의 각도 계산
            float angle = Vector3.Angle(playerForward, botDirection);

            // 5도 이내에 들어오면 대시
            if (angle <= 5f)
            {
                Debug.Log("대시 시작");
                Fsmplayer.Dash(); // FSMPlayer의 Dash 메서드 호출
                _bot.ChangeState(Bot.BotStateEnum.CHASING); // 대시 후 추적 상태로 전환
                return;
            }
        }

        // Power가 없거나 조건에 맞지 않으면 추적 상태로 전환
        Debug.Log("Dash 조건 불충족. Chasing 상태로 전환.");
        _bot.ChangeState(Bot.BotStateEnum.CHASING);
    }

    public override void OnStateEnd()
    {
        Debug.Log("===== End Dash =====");
    }
}
