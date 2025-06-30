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

    // Power �׵� ����
    protected bool isGetPower; // Power �Ծ����� �Ǻ�

    public bool GetIsGetPower()
    {
        return isGetPower;
    }

    // Player �̵� ������
    protected float inputValueY; // Y�� �Է� ��
    protected float rotationInput; // ȸ�� �Է� ��
    public Rigidbody2D body; // Rigidbody2D ������Ʈ
    public Collider2D playerCollider; // �÷��̾� �ڽ��� �ݶ��̴�

    // P_Attack ���� ����
    public Transform firePoint; // �߻� ��ġ(�÷��̾� ��)

    // Ring ���� ���� ��ü
    public GameObject ring;

    //Sword ��ü
    public GameObject sword;

    //sword animation ��Ʈ�ѷ�
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

        Debug.Log("player ���� ��ġ: " + playerData.initialPosition);
        Debug.Log("player ���� ȸ��: " + playerData.initialRotation);

        photonView.RPC("PowerRingState", RpcTarget.All, false, false); // �ʱ� ��, isPower ��Ȱ��

        isGetPower = false;

        //sword animator ����
        sword_animator = sword.GetComponent<Animator>();


        //���� ��Ÿ�� ����
        attack_curTime = 0.0f;

    }

    public void restPlayerPosition()
    {
        transform.position = getPosition();
        transform.rotation = getRotation();
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    // Get �޼����
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
        // ���� ������Ʈ���� Ȯ��
        if (!photonView.IsMine)
        {
            return;
        }


        // Ű �Է� ó��
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Attack ȣ��!");
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !playerData.onDash)
        {
            Debug.Log("Dash ȣ��!");
            photonView.RPC("Dash", RpcTarget.All);
        }

        // WŰ -> ������ �̵�
        inputValueY = Input.GetKey(KeyCode.W) ? 1f : 0f;
        // SŰ -> �ڷ� �̵�
        inputValueY += Input.GetKey(KeyCode.S) ? -1f : 0f;
        // AŰ -> �ð���� ȸ��
        rotationInput = Input.GetKey(KeyCode.A) ? 1f : 0f;
        // DŰ -> �ݽð���� ȸ��
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
        // ���� ������Ʈ���� Ȯ��
        if (!photonView.IsMine)
        {
            return;
        }
        // ȸ�� �ӵ� ������ 0���� ����
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
        // �̵� ���͸� ����Ͽ� ���� ȸ�� �������� �̵� (W/S Ű�� �̵�)
        Vector2 moveDirection = new Vector2(-Mathf.Sin(body.rotation * Mathf.Deg2Rad), Mathf.Cos(body.rotation * Mathf.Deg2Rad));
        body.velocity = moveDirection * inputValueY * playerData.playerSpeed;

        // RigidBody2D�� ȸ�� �� ���� (A/D Ű) - ���� ���� ���� ������
        body.MoveRotation(body.rotation + rotationInput * (playerData.playerSpeed + 100) * Time.fixedDeltaTime);
    }

    [PunRPC]
    public void SyncDashState(bool dashActive, bool colliderEnabled)
    {
        playerData.onDash = dashActive;
        playerCollider.enabled = colliderEnabled;
    }

    // ���� �޼���
    public void Attack()
    {
        if (attack_curTime <= 0 && !playerData.onDash)
        {
            sword_animator.SetTrigger("attack"); // Photon Animator View ���
            attack_curTime = attack_coolTime;
            if (playerData.isPower == true)
            {
                // �ִϸ��̼� ��� (Trigger�� ������� ����)
                //photonView.RPC("PlayPAttackAnimation", RpcTarget.All);
                photonView.RPC("P_Attack", RpcTarget.All); // P_Attack ����
            }
        }
    }

    [PunRPC]
    public void PlayPAttackAnimation()
    {
        sword_animator.Play("P_Attack", 0, 0f);
    }

    // Power ��Ȱ��ȭ
    [PunRPC]
    public void PowerRingState(bool rb, bool pb)
    {
        ring.SetActive(rb); // Ring ��Ȱ��ȭ
        playerData.isPower = pb; // Power �ٽ� �ʿ�
    }

    // Dash �޼���
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
                //Dash �� ����
                photonView.RPC("SyncDashState", RpcTarget.All, true, false);

                //�ٶ󺸴� �������� �̵��ӵ��� 2��� �̵�
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

            // �߻�ü ���� (firePoint ��ġ����)                 
            GameObject p_Attack = PhotonNetwork.Instantiate("Multi_P_Attack", firePoint.position, firePoint.rotation);

            // �߻�ü �ٶ󺸴� �������� �߻�
            p_Attack.GetComponent<Multi_P_Attack>().MoveP(firePoint.up);
        }
    }

    //Power ����
    [PunRPC]
    public void get_Power()
    {
        //�Ŀ��� �����ʾҴٸ� �Ŀ��� ����
        if (!playerData.isPower)
        {
            isGetPower = true;
            photonView.RPC("PowerRingState", RpcTarget.All, true, true);
            if (PhotonNetwork.IsMasterClient)
            {
                Multi_GameManager.instance.photonView.RPC("DiscountPower", RpcTarget.All);
            }
            Debug.Log(playerData.isPower + " Power ȹ��!");
        }
        else
        {
            isGetPower = false;
        }
    }

    // �浹 ó�� ������
    [PunRPC]
    public void Hit()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Multi_GameManager.instance.photonView.RPC("Hit_Handler", RpcTarget.All, playerData.playerType);
        }
    }

}
