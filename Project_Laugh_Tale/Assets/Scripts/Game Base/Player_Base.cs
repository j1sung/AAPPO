using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Base : MonoBehaviour
{
    [SerializeField] private PlayerData playerdata;
    public PlayerData playerData { get { return playerdata; } set { playerdata = value; } }

    // Player �̵� ������
    protected float inputValueY; // Y�� �Է� ��
    protected float rotationInput; // ȸ�� �Է� ��
    protected Rigidbody2D body; // Rigidbody2D ������Ʈ
    public Collider2D playerCollider; // �÷��̾� �ڽ��� �ݶ��̴�

    // P_Attack ���� ����
    [SerializeField] private GameObject p_AttackPrefab; // P_Attack ������
    public Transform firePoint; // �߻� ��ġ(�÷��̾� ��)

    // Ring ���� ���� ��ü
    public GameObject ring;

    //Sword ��ü
    public GameObject sword;

    //sword animation ��Ʈ�ѷ�
    private Animator sword_animator;

    //attack cool time
    private float attack_curTime;
    public float attack_coolTime = 0.5f;

    //Dash on time
    private float Dash_curTime;
    public float Dash_onTime = 0.1f;


    protected virtual void Awake()
    {
        ring.SetActive(false); // Ring ��,Ȱ��ȭ
        playerData.isPower = false; // Power ��,Ȱ��ȭ

        //Rigidbody2D ������Ʈ �޾ƿ�
        body = GetComponent<Rigidbody2D>();

        //sword animator ����
        sword_animator = sword.GetComponent<Animator>();

        //���� ��Ÿ�� ����
        attack_curTime = 0.0f;
    }

    public void restPlayerPosition()
    {
        transform.position = getPosition();
        transform.rotation = getRotation();
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

    public void setSpeed(float speed)
    {
        playerdata.playerSpeed = speed;
    }

    private void Update()
    {
        if (playerData.playerType != PlayerData.PlayerType.Enemy)
        {
            // Ű �Է� ó��
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack();
            }


            if (Input.GetKeyDown(KeyCode.LeftShift) && !playerData.onDash)
            {
                Dash();
            }

            // WŰ -> ������ �̵�
            inputValueY = Input.GetKey(KeyCode.W) ? 1f : 0f;
            // SŰ -> �ڷ� �̵�
            inputValueY += Input.GetKey(KeyCode.S) ? -1f : 0f;
            // AŰ -> �ð���� ȸ��
            rotationInput = Input.GetKey(KeyCode.A) ? 1f : 0f;
            // DŰ -> �ݽð���� ȸ��
            rotationInput += Input.GetKey(KeyCode.D) ? -1f : 0f;
        }
        attack_curTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // ȸ�� �ӵ� ������ 0���� ����
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

    //�̵� �Լ�
    public void Move()
    {
        if (playerData.onDash) return;
        // �̵� ���͸� ����Ͽ� ���� ȸ�� �������� �̵� (W/S Ű�� �̵�)
        Vector2 moveDirection = new Vector2(-Mathf.Sin(body.rotation * Mathf.Deg2Rad), Mathf.Cos(body.rotation * Mathf.Deg2Rad));
        body.velocity = moveDirection * inputValueY * playerData.playerSpeed;

        // RigidBody2D�� ȸ�� �� ���� (A/D Ű) - ���� ���� ���� ������
        body.MoveRotation(body.rotation + rotationInput * (playerData.playerSpeed + 100) * Time.fixedDeltaTime);
    }

    // ���� �޼���
    public void Attack()
    {
        if (attack_curTime <= 0 && !playerData.onDash)
        {
            sword_animator.SetTrigger("attack");
            attack_curTime = attack_coolTime;
            if (playerData.isPower == true)
            {
                // �ִϸ��̼� ��� (Trigger�� ������� ����)
                //sword_animator.Play("P_Attack", 0, 0f);
                P_Attack(); // P_Attack ����
            }
          
        }
    }

    // Dash �޼���
    public void Dash()
    {
        if (playerData.isPower == true)
        {
            Dash_curTime = 0f;
            playerData.onDash = true;
            ring.SetActive(false); // Ring ��,Ȱ��ȭ
            playerData.isPower = false; // Power ��,Ȱ��ȭ
        }
    }

    //Dash ���� �޼���
    public void Do_Dash()
    {
        if (Dash_curTime <= Dash_onTime)
        {
            playerCollider.enabled = false;

            //�ٶ󺸴� �������� �̵��ӵ��� 2��� �̵�
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
        ring.SetActive(false); // Ring ��,Ȱ��ȭ
        playerData.isPower = false; // Power ��,Ȱ��ȭ

        // �߻�ü ���� (firePoint ��ġ����)                       
        GameObject p_Attack = Instantiate(p_AttackPrefab, firePoint.position, firePoint.rotation);
        p_Attack.GetComponent<P_Attack_Base>().MoveP(firePoint.up);
    }

    //Power ����
    public bool get_Power()
    {
        //�Ŀ��� �����ʾҴٸ� �Ŀ��� ����
        if (!playerData.isPower)
        {
            ring.SetActive(true); // Ring ��,Ȱ��ȭ
            playerData.isPower = true; // Power ��,Ȱ��ȭ

            GameManager_Base.instance.DiscountPower();
            Debug.Log(playerData.isPower + " Power ȹ��!");
            return true;
        }
        else
        {
            return false;
        }
    }

    // �浹 ó��
    public void Hit()
    {
        GameManager_Base.instance.Hit_Handler(playerData);
    }
}
