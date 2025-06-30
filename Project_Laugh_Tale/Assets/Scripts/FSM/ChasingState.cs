using UnityEngine;

public class ChasingState : State
{
    private float attackAngleRange = 20f; // ������ ������ ���� ���� (20��)
    private float stateStartTime; // ���� ���� �ð�
    private float timeLimit = 1.0f; // ���� ���� �ð� ���� (2��)

    public ChasingState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Chasing ����...");
        stateStartTime = Time.time; // ���� ���� �ð� ���

        // �÷��̾ �ٶ󺸵��� ȸ��
        /*Vector3 directionToPlayer = (_bot.GetPlayerPosition() - _bot.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
        _bot.transform.rotation = targetRotation;*/
    }

    public override void OnStateUpdate()
    {
        // ���� ���� �ð� Ȯ��
        if (Time.time - stateStartTime > timeLimit)
        {
            Debug.Log("Chasing ���� �ð� �ʰ�, Patrolling���� ��ȯ.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING); // �ð� �ʰ� �� Patrolling���� ��ȯ
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

        
        // ���� ��ȯ ����
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
        Debug.Log("Chasing ����.");
    }
}
