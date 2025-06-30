using UnityEngine;

public class PatrollingState : State
{
    private float detectionRadius = 4f; // 탐지 범위

    public PatrollingState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Patrolling 시작");
    }

    public override void OnStateUpdate()
    {
        // Patrolling 중에도 주기적으로 탐지
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

        // FSMPlayer가 Power를 보유하고 있는 경우 아이템으로 이동하지 않음
        if (Fsmplayer.getPower())
        {
            Debug.Log("FSMPlayer가 Power를 보유 중입니다. Item 상태로 전환하지 않습니다.");
            _bot.ChangeState(Bot.BotStateEnum.CHASING); // 플레이어가 탐지되면 추적

            return; // Item 상태로 전환하지 않음
        }

        // 플레이어와 아이템의 우선순위 비교
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
        Debug.Log("Patrolling 종료");
    }
}
