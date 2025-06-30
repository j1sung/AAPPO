using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class Multi_Player : MonoBehaviourPun
{
    [SerializeField] private PlayerData_Multi playerdata;
    public PlayerData_Multi playerData { get { return playerdata; } set { playerdata = value; } }

    // Power 휙득 여부
    protected bool isGetPower; // Power 먹었는지 판별

    public bool GetIsGetPower()
    {
        return isGetPower;
    }

    // Player 이동 변수들
    protected float inputValueY; // Y축 입력 값
    protected float rotationInput; // 회전 입력 값
    public Rigidbody2D body; // Rigidbody2D 컴포넌트
    public Collider2D playerCollider; // 플레이어 자신의 콜라이더

    // P_Attack 공격 관련
    public Transform firePoint; // 발사 위치(플레이어 앞)

    // Ring 상태 설정 객체
    public GameObject ring;

    //Sword 객체
    public GameObject sword;

    //sword animation 컨트롤러
    public Animator sword_animator;

    //attack cool time
    private float attack_curTime;
    public float attack_coolTime = 0.5f;

    //Dash on time
    private float Dash_curTime;
    public float Dash_onTime = 0.1f;

    protected virtual void Awake()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        Debug.Log("player 생성 위치: " + playerData.initialPosition);
        Debug.Log("player 생성 회전: " + playerData.initialRotation);

        photonView.RPC("PowerRingState", RpcTarget.All, false, false); // 초기 링, isPower 비활성

        isGetPower = false;

        //sword animator 설정
        sword_animator = sword.GetComponent<Animator>();


        //공격 쿨타임 세팅
        attack_curTime = 0.0f;

    }

    public void restPlayerPosition()
    {
        transform.position = getPosition();
        transform.rotation = getRotation();
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
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

    private void Update()
    {
        // 로컬 오브젝트인지 확인
        if (!photonView.IsMine)
        {
            return;
        }


        // 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Attack 호출!");
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !playerData.onDash)
        {
            Debug.Log("Dash 호출!");
            photonView.RPC("Dash", RpcTarget.All);
        }

        // W키 -> 앞으로 이동
        inputValueY = Input.GetKey(KeyCode.W) ? 1f : 0f;
        // S키 -> 뒤로 이동
        inputValueY += Input.GetKey(KeyCode.S) ? -1f : 0f;
        // A키 -> 시계방향 회전
        rotationInput = Input.GetKey(KeyCode.A) ? 1f : 0f;
        // D키 -> 반시계방향 회전
        rotationInput += Input.GetKey(KeyCode.D) ? -1f : 0f;

        attack_curTime -= Time.deltaTime;
    }

    [PunRPC]
    public void SyncVelocity(Vector2 velocity, float angularVelocity)
    {
        if (!photonView.IsMine)
        {
            body.velocity = velocity;
            body.angularVelocity = angularVelocity;
        }
    }

    private void FixedUpdate()
    {
        // 로컬 오브젝트인지 확인
        if (!photonView.IsMine)
        {
            return;
        }
        // 회전 속도 강제로 0으로 설정
        if (body != null)
        {
            body.angularVelocity = 0f;
        }

        if (body != null)
        {
            Move();
        }
        if (playerData.onDash)
        {
            photonView.RPC("Do_Dash", RpcTarget.All);
        }
    }

    public void Move()
    {
        if (playerData.onDash) return;
        // 이동 벡터를 계산하여 현재 회전 방향으로 이동 (W/S 키로 이동)
        Vector2 moveDirection = new Vector2(-Mathf.Sin(body.rotation * Mathf.Deg2Rad), Mathf.Cos(body.rotation * Mathf.Deg2Rad));
        body.velocity = moveDirection * inputValueY * playerData.playerSpeed;

        // RigidBody2D의 회전 값 적용 (A/D 키) - 직접 각도 값을 더해줌
        body.MoveRotation(body.rotation + rotationInput * (playerData.playerSpeed + 100) * Time.fixedDeltaTime);
    }

    [PunRPC]
    public void SyncDashState(bool dashActive, bool colliderEnabled)
    {
        playerData.onDash = dashActive;
        playerCollider.enabled = colliderEnabled;
    }

    // 공격 메서드
    public void Attack()
    {
        if (attack_curTime <= 0 && !playerData.onDash)
        {
            sword_animator.SetTrigger("attack"); // Photon Animator View 사용
            attack_curTime = attack_coolTime;
            if (playerData.isPower == true)
            {
                // 애니메이션 재생 (Trigger는 사용하지 않음)
                //photonView.RPC("PlayPAttackAnimation", RpcTarget.All);
                photonView.RPC("P_Attack", RpcTarget.All); // P_Attack 공격
            }
        }
    }

    [PunRPC]
    public void PlayPAttackAnimation()
    {
        sword_animator.Play("P_Attack", 0, 0f);
    }

    // Power 비활성화
    [PunRPC]
    public void PowerRingState(bool rb, bool pb)
    {
        ring.SetActive(rb); // Ring 비활성화
        playerData.isPower = pb; // Power 다시 필요
    }

    // Dash 메서드
    [PunRPC]
    public void Dash()
    {
        if (playerData.isPower == true)
        {
            Dash_curTime = 0f;
            playerData.onDash = true;
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("PowerRingState", RpcTarget.All, false, false);
            }
        }
    }

    [PunRPC]
    public void MoveDash()
    {
        body.velocity = firePoint.up * playerData.Dash_speed;
    }

    [PunRPC]
    public void Do_Dash()
    {
        if (Dash_curTime <= Dash_onTime && playerData.onDash)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                //Dash 중 무적
                photonView.RPC("SyncDashState", RpcTarget.All, true, false);

                //바라보는 방향으로 이동속도의 2배로 이동
                photonView.RPC("MoveDash", RpcTarget.All);
            }

            Dash_curTime += Time.deltaTime;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SyncDashState", RpcTarget.All, false, true);
            }
        }
    }

    // P_Attack
    [PunRPC]
    public void P_Attack()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("PowerRingState", RpcTarget.All, false, false);

            // 발사체 생성 (firePoint 위치에서)                 
            GameObject p_Attack = PhotonNetwork.Instantiate("Multi_P_Attack", firePoint.position, firePoint.rotation);

            // 발사체 바라보는 방향으로 발사
            p_Attack.GetComponent<Multi_P_Attack>().MoveP(firePoint.up);
        }
    }

    //Power 습득
    [PunRPC]
    public void get_Power()
    {
        //파워를 먹지않았다면 파워를 습득
        if (!playerData.isPower)
        {
            isGetPower = true;
            photonView.RPC("PowerRingState", RpcTarget.All, true, true);
            if (PhotonNetwork.IsMasterClient)
            {
                Multi_GameManager.instance.photonView.RPC("DiscountPower", RpcTarget.All);
            }
            Debug.Log(playerData.isPower + " Power 획득!");
        }
        else
        {
            isGetPower = false;
        }
    }

    // 충돌 처리 구현부
    [PunRPC]
    public void Hit()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Multi_GameManager.instance.photonView.RPC("Hit_Handler", RpcTarget.All, playerData.playerType);
        }
    }

}
