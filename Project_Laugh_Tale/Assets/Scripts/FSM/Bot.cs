using UnityEngine;

public class Bot : Ai
{
    [SerializeField] public float playerSpeed;
    [SerializeField] public float attackRange;
    [SerializeField] public Vector3 targetPosition; // ���� ��ǥ ��ġ (Player �Ǵ� Power)
    private Player _player;
    public FSMPlayer _Fsmplayer;

    public enum BotStateEnum
    {
        IDLE,
        ATTACKING,
        PATROLLING,
        CHASING,
        ITEM,
        DASHING
    }

    public BotStateEnum _currentState;
    private FSM _fsm;

    private void Start()
    {
        // Bot�� ó���� Patrolling ���·� ����
        _currentState = BotStateEnum.PATROLLING;
        _fsm = new FSM(new PatrollingState(this));
    }

    public void reset_state()
    {
        _currentState = BotStateEnum.PATROLLING;
    }

    private void Update()
    {
        if(!_Fsmplayer.enabled)
        {
            reset_state();
            return;
        }
        // ���� ���� ������Ʈ
        _fsm.UpdateState();
    }

    public void ChangeState(BotStateEnum nextState)
    {
        _currentState = nextState;

        switch (_currentState)
        {
            case BotStateEnum.IDLE:
                _fsm.ChangeState(new IdleState(this));
                break;
            case BotStateEnum.PATROLLING:
                _fsm.ChangeState(new PatrollingState(this));
                break;
            case BotStateEnum.ATTACKING:
                _fsm.ChangeState(new AttackingState(this));
                break;
            case BotStateEnum.CHASING:
                _fsm.ChangeState(new ChasingState(this));
                break;
            case BotStateEnum.ITEM:
                _fsm.ChangeState(new ItemState(this));
                break;
            case BotStateEnum.DASHING:
                _fsm.ChangeState(new DashState(this));
                break;
            default:
                Debug.LogWarning($"Unknown state: {_currentState}");
                break;
        }
    }

    public Player GetPlayer()
    {
        if (_player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogError("Player object with tag 'Player' not found!");
                return null;
            }

            _player = playerObject.GetComponent<Player>();
            if (_player == null)
            {
                Debug.LogError("FSMPlayer component not found on Player object!");
            }
        }
        return _player;
    }

    public FSMPlayer GetFSM()
    {
        
        return _Fsmplayer;
    }

    public void Move(Vector3 targetPosition)
    {

        // ���� ��ġ�� ��ǥ ��ġ ���� ���� ���� ���
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetDirection = (new Vector2(targetPosition.x, targetPosition.y) - currentPosition).normalized;

        // ���������� �������� �Ǻ� (Z�� �������� Cross Product�� Z�� ���)
        float crossZ = Vector3.Cross(transform.up, targetDirection).z;

        // ȸ�� �Է°� ����
        float rotateInput = 0f;
        if (Mathf.Abs(crossZ) > 0.05f) // ȸ���� �ʿ��� ���
        {
            rotateInput = crossZ > 0 ? 1f : -1f; // �����̸� 1, �������̸� -1
        }

        // ���� �� ���� (ȸ�� ���̸� 0, ȸ�� �Ϸ� �� 1)
        float forwardInput = rotateInput == 0 ? 1f : 0f;
        // ���� ���� ����Ͽ� move ȣ��
        _Fsmplayer.move(forwardInput, rotateInput);
    }



    public bool IsPlayerInDashAngle(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
        float angle = Vector3.Angle(transform.up, directionToPlayer);
        return angle <= 20f; // 20�� �̳��� true ��ȯ
    }


    public Vector3 GetPlayerPosition()
    {
        if (_player != null)
        {
            return _player.transform.position;
        }
        return Vector3.zero;
    }

    public Vector3 GetPlayerup()
    {
        return _player.transform.up;
    }
}
