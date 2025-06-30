using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Base : MonoBehaviour
{
    [SerializeField] private PlayerData playerdata;
    public PlayerData playerData { get { return playerdata; } set { playerdata = value; } }

    // Player 이동 변수들
    protected float inputValueY; // Y축 입력 값
    protected float rotationInput; // 회전 입력 값
    protected Rigidbody2D body; // Rigidbody2D 컴포넌트
    public Collider2D playerCollider; // 플레이어 자신의 콜라이더

    // P_Attack 공격 관련
    [SerializeField] private GameObject p_AttackPrefab; // P_Attack 프리팹
    public Transform firePoint; // 발사 위치(플레이어 앞)

    // Ring 상태 설정 객체
    public GameObject ring;

    //Sword 객체
    public GameObject sword;

    //sword animation 컨트롤러
    private Animator sword_animator;

    //attack cool time
    private float attack_curTime;
    public float attack_coolTime = 0.5f;

    //Dash on time
    private float Dash_curTime;
    public float Dash_onTime = 0.1f;


    protected virtual void Awake()
    {
        ring.SetActive(false); // Ring 비,활성화
        playerData.isPower = false; // Power 비,활성화

        //Rigidbody2D 컴포넌트 받아옴
        body = GetComponent<Rigidbody2D>();

        //sword animator 설정
        sword_animator = sword.GetComponent<Animator>();

        //공격 쿨타임 세팅
        attack_curTime = 0.0f;
    }

    public void restPlayerPosition()
    {
        transform.position = getPosition();
        transform.rotation = getRotation();
    }

    // Get 메서드들
    public Vector2 getPosition()
    {
        return playerData.initialPosition;
    }

    public Quaternion getRotation()
    {
        return playerData.initialRotation;
    }

    public void setSpeed(float speed)
    {
        playerdata.playerSpeed = speed;
    }

    private void Update()
    {
        if (playerData.playerType != PlayerData.PlayerType.Enemy)
        {
            // 키 입력 처리
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack();
            }


            if (Input.GetKeyDown(KeyCode.LeftShift) && !playerData.onDash)
            {
                Dash();
            }

            // W키 -> 앞으로 이동
            inputValueY = Input.GetKey(KeyCode.W) ? 1f : 0f;
            // S키 -> 뒤로 이동
            inputValueY += Input.GetKey(KeyCode.S) ? -1f : 0f;
            // A키 -> 시계방향 회전
            rotationInput = Input.GetKey(KeyCode.A) ? 1f : 0f;
            // D키 -> 반시계방향 회전
            rotationInput += Input.GetKey(KeyCode.D) ? -1f : 0f;
        }
        attack_curTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // 회전 속도 강제로 0으로 설정
        if (body != null)
        {
            body.angularVelocity = 0f;
        }
        if (body != null && playerData.playerType != PlayerData.PlayerType.Enemy)
        {
            Move();
        }

        if (playerData.onDash)
        {
            Do_Dash();
        }
    }

    //이동 함수
    public void Move()
    {
        if (playerData.onDash) return;
        // 이동 벡터를 계산하여 현재 회전 방향으로 이동 (W/S 키로 이동)
        Vector2 moveDirection = new Vector2(-Mathf.Sin(body.rotation * Mathf.Deg2Rad), Mathf.Cos(body.rotation * Mathf.Deg2Rad));
        body.velocity = moveDirection * inputValueY * playerData.playerSpeed;

        // RigidBody2D의 회전 값 적용 (A/D 키) - 직접 각도 값을 더해줌
        body.MoveRotation(body.rotation + rotationInput * (playerData.playerSpeed + 100) * Time.fixedDeltaTime);
    }

    // 공격 메서드
    public void Attack()
    {
        if (attack_curTime <= 0 && !playerData.onDash)
        {
            sword_animator.SetTrigger("attack");
            attack_curTime = attack_coolTime;
            if (playerData.isPower == true)
            {
                // 애니메이션 재생 (Trigger는 사용하지 않음)
                //sword_animator.Play("P_Attack", 0, 0f);
                P_Attack(); // P_Attack 공격
            }
          
        }
    }

    // Dash 메서드
    public void Dash()
    {
        if (playerData.isPower == true)
        {
            Dash_curTime = 0f;
            playerData.onDash = true;
            ring.SetActive(false); // Ring 비,활성화
            playerData.isPower = false; // Power 비,활성화
        }
    }

    //Dash 진행 메서드
    public void Do_Dash()
    {
        if (Dash_curTime <= Dash_onTime)
        {
            playerCollider.enabled = false;

            //바라보는 방향으로 이동속도의 2배로 이동
            body.velocity = firePoint.up * playerData.Dash_speed;

            Dash_curTime += Time.deltaTime;
        }
        else
        {
            playerCollider.enabled = true;
            playerData.onDash = false;
        }
    }

    // P_Attack
    public void P_Attack()
    {
        ring.SetActive(false); // Ring 비,활성화
        playerData.isPower = false; // Power 비,활성화

        // 발사체 생성 (firePoint 위치에서)                       
        GameObject p_Attack = Instantiate(p_AttackPrefab, firePoint.position, firePoint.rotation);
        p_Attack.GetComponent<P_Attack_Base>().MoveP(firePoint.up);
    }

    //Power 습득
    public bool get_Power()
    {
        //파워를 먹지않았다면 파워를 습득
        if (!playerData.isPower)
        {
            ring.SetActive(true); // Ring 비,활성화
            playerData.isPower = true; // Power 비,활성화

            GameManager_Base.instance.DiscountPower();
            Debug.Log(playerData.isPower + " Power 획득!");
            return true;
        }
        else
        {
            return false;
        }
    }

    // 충돌 처리
    public void Hit()
    {
        GameManager_Base.instance.Hit_Handler(playerData);
    }
}
