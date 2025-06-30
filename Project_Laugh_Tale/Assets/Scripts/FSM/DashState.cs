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

        // ó�� ���� �� ��� ���� Ȯ�� �� ó��
        HandleDashOrSwitchState();
    }

    public override void OnStateUpdate()
    {
        // ��� ������ ���������� Ȯ��
        HandleDashOrSwitchState();
    }

    private void HandleDashOrSwitchState()
    {
        if (Fsmplayer.getPower()) // Power�� ������ �ִ� ���
        {
            Vector3 playerPosition = _bot.GetPlayerPosition();

            // �÷��̾ �ٶ󺸴� ����� Bot�� ��� ���� ���
            Vector3 playerForward = _bot.GetPlayerup(); // �÷��̾ �ٶ󺸴� ����
            Vector3 botDirection = _bot.transform.position - playerPosition; // Bot ��ġ���� �÷��̾������ ����
            // �÷��̾ ���� ����� Bot�� ���� ���� ���� ���
            float angle = Vector3.Angle(playerForward, botDirection);

            // 5�� �̳��� ������ ���
            if (angle <= 5f)
            {
                Debug.Log("��� ����");
                Fsmplayer.Dash(); // FSMPlayer�� Dash �޼��� ȣ��
                _bot.ChangeState(Bot.BotStateEnum.CHASING); // ��� �� ���� ���·� ��ȯ
                return;
            }
        }

        // Power�� ���ų� ���ǿ� ���� ������ ���� ���·� ��ȯ
        Debug.Log("Dash ���� ������. Chasing ���·� ��ȯ.");
        _bot.ChangeState(Bot.BotStateEnum.CHASING);
    }

    public override void OnStateEnd()
    {
        Debug.Log("===== End Dash =====");
    }
}
