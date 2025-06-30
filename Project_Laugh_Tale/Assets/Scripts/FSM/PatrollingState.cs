using UnityEngine;

public class PatrollingState : State
{
    private float detectionRadius = 4f; // Ž�� ����

    public PatrollingState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Patrolling ����");
    }

    public override void OnStateUpdate()
    {
        // Patrolling �߿��� �ֱ������� Ž��
        DetectPlayerAndPower();
    }

    private void DetectPlayerAndPower()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_bot.transform.position, detectionRadius);
        Collider2D closestPower = null;
        float closestPowerDistance = Mathf.Infinity;
        Player player = null;
        float playerDistance = Mathf.Infinity;

        FSMPlayer Fsmplayer = _bot.GetFSM();

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                player = collider.GetComponent<Player>();
                playerDistance = Vector3.Distance(_bot.transform.position, collider.transform.position);
            }
            else if (collider.CompareTag("Power"))
            {
                float distance = Vector3.Distance(_bot.transform.position, collider.transform.position);
                if (distance < closestPowerDistance)
                {
                    closestPowerDistance = distance;
                    closestPower = collider;
                }
            }
        }

        // FSMPlayer�� Power�� �����ϰ� �ִ� ��� ���������� �̵����� ����
        if (Fsmplayer.getPower())
        {
            Debug.Log("FSMPlayer�� Power�� ���� ���Դϴ�. Item ���·� ��ȯ���� �ʽ��ϴ�.");
            _bot.ChangeState(Bot.BotStateEnum.CHASING); // �÷��̾ Ž���Ǹ� ����

            return; // Item ���·� ��ȯ���� ����
        }

        // �÷��̾�� �������� �켱���� ��
        if (player != null && closestPower != null)
        {
            if (playerDistance < closestPowerDistance)
            {
                _bot.ChangeState(Bot.BotStateEnum.CHASING);
            }
            else
            {
                _bot.ChangeState(Bot.BotStateEnum.ITEM);
                _bot.targetPosition = closestPower.transform.position;
            }
        }
        else if (player != null && playerDistance <= detectionRadius)
        {
            _bot.ChangeState(Bot.BotStateEnum.CHASING);
        }
        else if (closestPower != null && closestPowerDistance <= detectionRadius)
        {
            _bot.ChangeState(Bot.BotStateEnum.ITEM);
            _bot.targetPosition = closestPower.transform.position;
        }
    }

    public override void OnStateEnd()
    {
        Debug.Log("Patrolling ����");
    }
}
