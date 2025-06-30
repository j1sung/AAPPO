using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;               //ML-Agents 사용을 위한 namespace
using Unity.MLAgents.Sensors;       //관즉정보 사용을 위한 namespace
using Unity.MLAgents.Actuators;     //python으로 부터 행동을 받기위한 namespace

public class Player_Player_Agent : Agent
{
    public GameObject enemy;    //같은 환경에서 자신의 경쟁 상대
    private Player enemy_player;
    private PlayerData enemy_playerData;

    //자신의 데이터

    private Rigidbody RbAgent;
    private PlayerData playerData;
    public Player player;

    //행동 상수
    private const int Stay = 0;
    private const int Forward = 1;
    private const int Backward = 2;
    private const int Turn_Left = 3;
    private const int Turn_Right = 4;
    private const int Attack = 5;
    private const int Dash = 6;

    //에이전트 초기 위치 설정
    private Vector3 ResetPosAgent;

    //버퍼 센서
    public BufferSensorComponent P_BufferSensor;
    public BufferSensorComponent S_BufferSensor;
    List<float> sensorDistList = new List<float>();

    //초기화
    public override void Initialize()
    {
        RbAgent = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        playerData = player.playerData;
        enemy_player = enemy.GetComponent<Player>();
        enemy_playerData = enemy_player.playerData;
        ResetPosAgent = playerData.initialPosition;
        Academy.Instance.AgentPreStep += WaitTimeInference; //Decision 리퀘스트 설정
    }

    //벡터 관측 정보 수집
    public override void CollectObservations(VectorSensor sensor)
    {
        //Power의 위치 정보를 추가
        GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
        foreach (GameObject power in existingPower)
        {
            sensorDistList.Clear();
            sensorDistList.Add(power.transform.localPosition.x);
            sensorDistList.Add(power.transform.localPosition.y);
            P_BufferSensor.AppendObservation(sensorDistList.ToArray());
        }

        //투사체의 위치정보를 추가
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

        //player의 위치, 바라보는 방향, 파워 습득 여부 추가
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        sensor.AddObservation(playerData.isPower);

        //enemy의 위치, 바라보는 방향, 파워 습득 여부 추가
        sensor.AddObservation(enemy.transform.localPosition.x);
        sensor.AddObservation(enemy.transform.localPosition.z);
        sensor.AddObservation(enemy.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        sensor.AddObservation(enemy_playerData.isPower);

    }

    //행동
    public override void OnActionReceived(ActionBuffers actionsBuffers)
    {
        if (!player.enabled) { return; }
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

    //에피소드가 실행 될때마다 호출, 초기화
    public override void OnEpisodeBegin()
    {
    }
    
    //휴리스틱 함수, 테스트용, 모방 학습용
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        /*
        var discreteAactionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W)) discreteAactionsOut[0] = 1;
        if (Input.GetKey(KeyCode.S)) discreteAactionsOut[0] = 2;
        if (Input.GetKey(KeyCode.A)) discreteAactionsOut[0] = 3;
        if (Input.GetKey(KeyCode.D)) discreteAactionsOut[0] = 4;
        if (Input.GetKeyDown(KeyCode.Space)) discreteAactionsOut[0] = 5;
        if (Input.GetKeyDown(KeyCode.LeftShift)) discreteAactionsOut[0] = 6;
        */
    }

    //Decision Requset 관련 설정
    float DecisionWaitingTime = 0.02f;
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
