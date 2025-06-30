using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingState : State
{
    private float attackAngleRange = 5f; // ���� ���� ����
    private FSMPlayer Fsmplayer;

    public AttackingState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Attacking: ����");
        Fsmplayer = _bot.GetFSM();

        // ù ��° ���� �� ���� Ȯ�� �� ó��
        HandleAttackOrSwitchState();
    }

    public override void OnStateUpdate()
    {
        // ���������� ���� Ȯ�� �� ��ȯ ó��
        HandleAttackOrSwitchState();
    }

    private void HandleAttackOrSwitchState()
    {
        // �÷��̾���� �Ÿ� �� ���� ���
        float distanceToPlayer = Vector3.Distance(_bot.transform.position, _bot.GetPlayerPosition());
        Vector3 directionToPlayer = (_bot.GetPlayerPosition() - _bot.transform.position).normalized;
        float angleToPlayer = Vector3.Angle(_bot.transform.up, directionToPlayer);

        // �÷��̾ �ٶ󺸵��� ȸ��
        RotateTowardsPlayer(_bot.GetPlayerPosition());

        // ���� 1: FSM�� Power�� ������ �ְ�, �÷��̾ 20�� ���� ���� �ִ� ���
        if (Fsmplayer.getPower() && angleToPlayer <= attackAngleRange)
        {
            Debug.Log("Condition 1: Power is active and Player is within angle range.");
            Fsmplayer.Attack(); // FSMPlayer�� Attack �޼��� ����
            _bot.ChangeState(Bot.BotStateEnum.CHASING); // ���� �� ���� ���·� ��ȯ
            return;
        }

        // ���� 2: �÷��̾ ��Ÿ� �ȿ� ���� ���
        if (distanceToPlayer <= _bot.attackRange)
        {
            Debug.Log("Condition 2: Player is within attack range.");
            Fsmplayer.Attack(); // FSMPlayer�� Attack �޼��� ����
        }
        else
        {
            Debug.Log("Player is out of range or angle. Switching to Chasing state.");
            _bot.ChangeState(Bot.BotStateEnum.CHASING); // ��Ÿ����� ����� ���� ���·� ��ȯ
        }
    }

    private void RotateTowardsPlayer(Vector3 directionToPlayer)
    {
        _bot.Move(directionToPlayer);
    }

    public override void OnStateEnd()
    {
        Debug.Log("Attacking: ����");
    }
}
