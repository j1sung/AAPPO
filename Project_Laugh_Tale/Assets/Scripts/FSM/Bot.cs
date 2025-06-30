using UnityEngine;

public class Bot : Ai
{
    [SerializeField] public float playerSpeed;
    [SerializeField] public float attackRange;
    [SerializeField] public Vector3 targetPosition; // 현재 목표 위치 (Player 또는 Power)
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
        // Bot은 처음에 Patrolling 상태로 시작
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
        // 현재 상태 업데이트
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

        // 현재 위치와 목표 위치 간의 방향 벡터 계산
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetDirection = (new Vector2(targetPosition.x, targetPosition.y) - currentPosition).normalized;

        // 오른쪽인지 왼쪽인지 판별 (Z축 기준으로 Cross Product의 Z값 사용)
        float crossZ = Vector3.Cross(transform.up, targetDirection).z;

        // 회전 입력값 결정
        float rotateInput = 0f;
        if (Mathf.Abs(crossZ) > 0.05f) // 회전이 필요한 경우
        {
            rotateInput = crossZ > 0 ? 1f : -1f; // 왼쪽이면 1, 오른쪽이면 -1
        }

        // 전진 값 설정 (회전 중이면 0, 회전 완료 시 1)
        float forwardInput = rotateInput == 0 ? 1f : 0f;
        // 계산된 값을 사용하여 move 호출
        _Fsmplayer.move(forwardInput, rotateInput);
    }



    public bool IsPlayerInDashAngle(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
        float angle = Vector3.Angle(transform.up, directionToPlayer);
        return angle <= 20f; // 20도 이내면 true 반환
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
