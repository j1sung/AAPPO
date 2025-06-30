using UnityEngine;

public class ItemState : State
{
    private float stateStartTime; // ���� ���� �ð�
    private float timeLimit = 1.0f; // ���� ���� �ð� ���� (2��)

    public ItemState(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    public override void OnStateStart()
    {
        Debug.Log("Item ���� ����...");
        stateStartTime = Time.time; // ���� ���� �ð� ���
    }

    public override void OnStateUpdate()
    {
        // ���� ���� �ð� Ȯ��
        if (Time.time - stateStartTime > timeLimit)
        {
            Debug.Log("Item ���� �ð� �ʰ�, Patrolling���� ��ȯ.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING); // �ð� �ʰ� �� Patrolling���� ��ȯ
            return;
        }

        Vector3 powerPosition = _bot.targetPosition;
        if (powerPosition == null)
        {
            Debug.LogWarning("Power Position is null.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING); // ��ġ ������ ������ Patrolling���� ��ȯ
            return;
        }

        // ��ǥ ���� ���
        Vector3 directionToPower = (powerPosition - _bot.transform.position).normalized;

        // �Ų����� ȸ��
        //Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPower);
        //_bot.transform.rotation = Quaternion.Slerp(_bot.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // ��ǥ ��ġ�� �̵�
        _bot.Move(powerPosition);

        // ��ǥ�� �����ϸ� ���� ����
        if (Vector3.Distance(_bot.transform.position, powerPosition) < 0.1f)
        {
            Debug.Log("Power�� ����, Patrolling���� ��ȯ.");
            _bot.ChangeState(Bot.BotStateEnum.PATROLLING);
        }
    }

    public override void OnStateEnd()
    {
        Debug.Log("Item ���� ����.");
    }
}
