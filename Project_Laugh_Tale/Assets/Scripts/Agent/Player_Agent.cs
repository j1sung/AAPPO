using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;               //ML-Agents ����� ���� namespace
using Unity.MLAgents.Sensors;       //�������� ����� ���� namespace
using Unity.MLAgents.Actuators;     //python���� ���� �ൿ�� �ޱ����� namespace

public class Player_Agent : Agent
{
    public GameObject enemy;    //���� ȯ�濡�� �ڽ��� ���� ���
    private Player enemy_player;
    private PlayerData enemy_playerData;

    private Rigidbody RbAgent;
    private PlayerData playerData;
    private Player player;

    //�ൿ ���
    private const int Stay = 0;
    private const int Forward = 1;
    private const int Backward = 2;
    private const int Turn_Left = 3;
    private const int Turn_Right = 4;
    private const int Attack = 5;
    private const int Dash = 6;

    //������Ʈ �ʱ� ��ġ ����
    private Vector3 ResetPosAgent;

    //���� ����
    public BufferSensorComponent P_BufferSensor;
    public BufferSensorComponent S_BufferSensor;
    List<float> sensorDistList = new List<float>();

    //�ʱ�ȭ
    public override void Initialize()
    {
        RbAgent = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        playerData = player.playerData;
        enemy_player = enemy.GetComponent<Player>();
        enemy_playerData = enemy_player.playerData;
        ResetPosAgent = playerData.initialPosition;
        Academy.Instance.AgentPreStep += WaitTimeInference; //Decision ������Ʈ ����
    }

    //���� ���� ���� ����
    public override void CollectObservations(VectorSensor sensor)
    {
        //Power�� ��ġ ������ �߰�
        GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
        foreach (GameObject power in existingPower)
        {
            sensorDistList.Clear();
            sensorDistList.Add(power.transform.localPosition.x);
            sensorDistList.Add(power.transform.localPosition.y);
            P_BufferSensor.AppendObservation(sensorDistList.ToArray());
        }

        //����ü�� ��ġ������ �߰�
        GameObject[] existingSword_aura = GameObject.FindGameObjectsWithTag("P_Attack");
        foreach (GameObject Sword_aura in existingSword_aura)
        {
            sensorDistList.Clear();
            sensorDistList.Add(Sword_aura.transform.localPosition.x);
            sensorDistList.Add(Sword_aura.transform.localPosition.y);
            sensorDistList.Add(Sword_aura.GetComponent<Rigidbody2D>().velocity.x);
            sensorDistList.Add(Sword_aura.GetComponent<Rigidbody2D>().velocity.y);
            S_BufferSensor.AppendObservation(sensorDistList.ToArray());
        }

        //player�� ��ġ, �ٶ󺸴� ����, �Ŀ� ���� ���� �߰�
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        sensor.AddObservation(playerData.isPower);

        //enemy�� ��ġ, �ٶ󺸴� ����, �Ŀ� ���� ���� �߰�
        sensor.AddObservation(enemy.transform.localPosition.x);
        sensor.AddObservation(enemy.transform.localPosition.z);
        sensor.AddObservation(enemy.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        sensor.AddObservation(enemy_playerData.isPower);

    }

    //�ൿ
    public override void OnActionReceived(ActionBuffers actionsBuffers)
    {
        var vectorAction = actionsBuffers.DiscreteActions;

        int action = Mathf.FloorToInt(vectorAction[0]);

        switch (action)
        {
            case Stay:
                player.move(0f, 0f);
                break;
            case Forward:
                player.move(1f, 0f);
                break;
            case Backward:
                player.move(-1f, 0f);
                break;
            case Turn_Left:
                player.move(0f, 1f);
                break;
            case Turn_Right:
                player.move(0f, -1f);
                break;
            case Attack:
                player.Attack();
                break;
            case Dash:
                player.Dash();
                break;
        }
    }

    //���Ǽҵ尡 ���� �ɶ����� ȣ��, �ʱ�ȭ
    public override void OnEpisodeBegin()
    {
    }
    
    //�޸���ƽ �Լ�, �׽�Ʈ��, ��� �н���
    public override void Heuristic(in ActionBuffers actionsOut)
    {
 
    }

    //Decision Requset ���� ����
    float DecisionWaitingTime = 0.001f;
    float m_currentTime = 0f;

    public void WaitTimeInference(int action)
    {
        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
        else
        {
            if (m_currentTime >= DecisionWaitingTime)
            {
                m_currentTime = 0f;
                RequestDecision();
            }
            else
            {
                m_currentTime += Time.fixedDeltaTime;
            }
        }
    }
}
