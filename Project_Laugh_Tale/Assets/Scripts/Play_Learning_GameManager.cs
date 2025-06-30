using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;

public class Play_Learning_GameManager : GameManager
{
    // Player 객체 참조 변수
    public GameObject humanPlayer_object;
    public GameObject enemyPlayer_object;

    // Player 객체 참조 변수
    public Player humanPlayer;
    public Player enemyPlayer;

    //Player Agent 참조
    public Agent enemy_Agent;

    //step 기록 용
    private int resetTimer;
    public int MaxEnvironmentSteps;

    // Player 충돌 순서
    public enum PlayerSequence { FirstPlayer, SecondPlayer }
    public PlayerSequence playerSequence;
    private PlayerData.PlayerType firstHitPlayerType;

    // UI - Power 프리펩 출력 관련
    public GameObject PowerPrefab;  // Power 프리팹
    [SerializeField] private int powerNumber = 7;  // Power 7개 생성
    public Camera mainCamera;    // 카메라 참조
    private int Power_cnt = 0;  //생성한 Power 개수
    public float spawnRateMin = 0.5f;   //생성 최소 주기
    public float spawnRateMax = 3.0f;   //생성 최대 주시
    private float timeAfterSpawn = 0.0f; //다음 생성 대기 시간
    private float spawnRate;    //생성 주기
    public float spawnDistance = 1.5f;  //생성을 피하는 범위

    // Score 관련
    private int humanScore = 0; // humanPlayer Score 변수
    private int enemyScore = 0; // enemyPlayer Score 변수

    //Score change 기록
    private bool human_score_change;
    private bool enemy_score_change;
    public override int GetHumanScore()
    {
        return humanScore;
    }
    public override int GetEnemyScore()
    {
        return enemyScore;
    }

    public override int GetRestTimer()
    {
        return resetTimer;
    }

    // 플레이어 충돌(무승부) 분기 관련
    public bool isProcessingCollision = false; // 충돌 처리 중인지 여부
    public const float delayThreshold = 0.1f;  // 100ms 딜레이 (동시 충돌 여부 확인)
    public bool isTwo = false; // 두번째 충돌 발생 여부

    // Round 전환 관련
    public int currentRound; // 현재 라운드
    public bool isRoundTransitioning = false; // 라운드 이동 중인지 여부

    void Start()
    {
        Debug.Log("Start");

        ResetGame();
    }

    // 플레이어 점수 올리는 메서드
    public void UpdateScore(PlayerData playerData)
    {
        PlayerData.PlayerType attackerType = playerData.playerType;
        if (attackerType == PlayerData.PlayerType.Human || attackerType == PlayerData.PlayerType.Human_AI)
        {
            //휴먼이 공격 받음
            enemy_Agent.AddReward(10f);
            enemy_score_change = true;
            enemyScore++;
        }
        else if (attackerType == PlayerData.PlayerType.Enemy || attackerType == PlayerData.PlayerType.Enemy_AI)
        {
            //적이 공격 받음
            enemy_Agent.AddReward(-10f);
            human_score_change = true;
            humanScore++;
        }
    }

    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            if(!enemy_score_change) enemy_Agent.AddReward(-5f);

            enemy_Agent.EpisodeInterrupted();
            //EpisodeInterruoted는 에피소드의 최대길이 도달 시 종료
            //가장 마지막 스텝에 대한 정보가 학습에 포함되지 않기 위함
            ResetGame();
        }
        else
        {
            // 게임 시작! (gameStarted == True)
            if (!gameStarted) //자동으로 시작
            {
                gameStarted = true;
                StartCoroutine(ShowReadyFight());
            }
            else
            {
                check_distance();
                check_dir();
                //마지막 파워 생성 후 시간이 얼마나 흘렀는지 체크
                timeAfterSpawn += Time.deltaTime;

                //생성 시간이 지난 후, 아직 파워 개수가 안차면 파워 다시 만들기
                if (timeAfterSpawn > spawnRate && Power_cnt < powerNumber)
                {
                    timeAfterSpawn = 0f;
                    SpawnPowers();
                    spawnRate = Random.Range(spawnRateMin, spawnRateMax);
                }
                else if (Power_cnt >= powerNumber)
                {
                    timeAfterSpawn = 0f;
                }
            }
        }
    }

    private void check_distance()
    {
        float dist = Vector3.Distance(humanPlayer.transform.position, enemyPlayer.transform.position);
        //공격 범위 내의 들어왔을 시 보상
        if ( 1.0f <= dist && 1.5f >= dist)
        {
            enemy_Agent.AddReward(0.0002f);
        }
        else
        {
            float alpha = (1.5f - dist)/22000.0f;
            enemy_Agent.AddReward(alpha);
        }
    }

    private void check_dir()
    {
        Vector3 human_watching_vector = (enemyPlayer.transform.position - humanPlayer.transform.position).normalized;
        Vector3 enemy_watching_vector = (humanPlayer.transform.position - enemyPlayer.transform.position).normalized;
        float human_dot = Vector3.Dot(humanPlayer.transform.up, human_watching_vector);
        float enemy_dot = Vector3.Dot(enemyPlayer.transform.up, enemy_watching_vector);

        float human_theta = Mathf.Acos(human_dot) * Mathf.Rad2Deg;
        float enemy_theta = Mathf.Acos(enemy_dot) * Mathf.Rad2Deg;

        if(human_theta <= 5.0f)
        {
            enemy_Agent.AddReward(-0.0004f);
        }

        if (enemy_theta <= 5.0f)
        {
            enemy_Agent.AddReward(0.0002f);
        }
        else
        {
            enemy_Agent.AddReward(-0.0002f);
        }
    }
    public override void discountPower()
    {
        if (Power_cnt > 0)
        {
            Power_cnt--;
        }
    }

    // Power 프리펩 화면 생성!
    void SpawnPowers()
    {
        Vector2 mapCenter = new Vector2(0, 0);  // 맵 중심
        float mapRadius = 3.5f;                   // 맵 반지름
        // 원형 맵 내부의 랜덤 위치에 생성
        Vector2 powerPosition = GetRandomPositionInCircle(mapCenter, mapRadius);

        Instantiate(PowerPrefab, powerPosition, Quaternion.identity);
        Power_cnt++;
    }

    private Vector2 GetRandomPositionInCircle(Vector2 center, float radius)
    {
        Vector2 spawnPosition;
        //human과 enemy 사이의 거리를 측정하여 일정 거리 안이라면 다시 생성좌표를 생성
        do
        {
            // 랜덤한 각도 (0 ~ 360도) 생성
            float angle = Random.Range(0f, Mathf.PI * 2);

            // 랜덤한 거리 (0 ~ radius) 생성
            float distance = Random.Range(0f, radius);

            // Polar Coordinates를 Cartesian Coordinates로 변환
            float x = center.x + Mathf.Cos(angle) * distance;
            float y = center.y + Mathf.Sin(angle) * distance;

            spawnPosition.x = x;
            spawnPosition.y = y;
        }
        while (Vector2.Distance(humanPlayer.transform.position, spawnPosition) <= spawnDistance ||
            Vector2.Distance(enemyPlayer.transform.position, spawnPosition) <= spawnDistance);
        return spawnPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(humanPlayer.transform.position, spawnDistance);
        Gizmos.DrawWireSphere(enemyPlayer.transform.position, spawnDistance);
    }

    void ClearPower()
    {
        // "Power" 태그를 가진 오브젝트를 모두 찾아서 삭제
        GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
        foreach (GameObject power in existingPower)
        {
            Destroy(power);
        }
        Power_cnt = 0;
    }

    //hit 판정을 하는 함수
    public override void Hit_Handler(PlayerData playerdata)
    {
        // 검이나 검기 충돌 처리
        // 만약 라운드 이동 중이라면 충돌 무시
        if (isRoundTransitioning)
        {
            Debug.Log("라운드 이동 중이므로 충돌 무시");
            return;
        }

        // 첫 번째 충돌을 기록하고 처리 시작
        if (!isProcessingCollision) // isProcessingCollision == false 일때
        {
            firstHitPlayerType = playerdata.playerType;
            isProcessingCollision = true; // 충돌 처리여부 true 변경
            isTwo = false; // 두 번째 충돌 여부 초기화

            StartCoroutine(HandleCollisionWithDelay(playerdata));
        }
        else // 이미 충돌을 처리 중이면 두번째 충돌 처리
        {
            if (firstHitPlayerType == playerdata.playerType)
            {
                return;
            }
            else
            {
                isTwo = true; // 두번째 플레이어 충돌 인식   
            }      
        }
    }

    // 두번째 충돌이 발생한다면 첫번째 충돌은 대기하며 두번째 충돌을 기다리고 NextRound()를 최종 호출하는 코루틴
    private IEnumerator HandleCollisionWithDelay(PlayerData firstPlayerdata)
    {
        //UI.instance.playerSequence = (UI.PlayerSequence)firstPlayerType; // 첫번째 충돌 플레이어가 human인지 enemy인지 구별

        // 딜레이 시간 동안 대기하며 두 번째 충돌 대기
        yield return new WaitForSeconds(delayThreshold);

        if (isTwo == true)
        {
            UpdateScore(humanPlayer.playerData);
            UpdateScore(enemyPlayer.playerData);
            Debug.Log("동시에 충돌이 일어났습니다!");
        }
        else
        {
            UpdateScore(firstPlayerdata);
            Debug.Log("한번의 충돌이 일어났습니다!");
        }

        //충돌 초기화
        isProcessingCollision = false;
        isTwo = false;

        NextRound(); // UI의 NextRound 호출
    }

    // 한 라운드가 끝날 때 호출하여 다음 라운드를 진행 함수
    public void NextRound()  // 공격에 성공한 플레이어의 점수를 1개 올리고 호출됨
    {
        isRoundTransitioning = true;
        isProcessingCollision = false;

        // "P_Attack" 태그를 가진 모든 게임 오브젝트를 찾아 삭제
        GameObject[] p_AttackTag = GameObject.FindGameObjectsWithTag("P_Attack");

        foreach (GameObject obj in p_AttackTag)
        {
            Destroy(obj); // 각 오브젝트를 삭제
        }
        ClearPower();

        // 둘 중 하나가 먼저 3점에 도달했고, 점수가 다르면 이긴 상황 (승패 결정)
        if ((humanScore >= 3 || enemyScore >= 3) && (humanScore != enemyScore))
        {
            if(humanScore >= 3)
            {
                //human이 이긴 경우 보상 부여
                enemy_Agent.AddReward(-10f);
                enemy_Agent.EndEpisode();
            }
            else
            {
                enemy_Agent.AddReward(10f);
                enemy_Agent.EndEpisode();
            }
            StartCoroutine(ShowKO());
            StartCoroutine(ShowEnd());
            humanScore = 0;
            enemyScore = 0;
            ResetGame();
        }
        else // 비긴 상황이거나, 둘 다 3점 미만인 경우 (게임이 아직 끝나지 않음)
        {
            currentRound++;   // 라운드 수 증가
            StartCoroutine(ShowKO());
            ResetGameState(); // 게임 상태를 초기화 시킴
        }
        isRoundTransitioning = false;
    }

    // 게임 상태만 초기화 - UI
    public void ResetGameState()
    {
        UI_Base.instance.UpdateRoundText();
        UI_Base.instance.UpdateScoreText();

        // 플레이어 위치 초기화
        humanPlayer.restPlayerPosition();
        enemyPlayer.restPlayerPosition();

        gameStarted = false;
        timeAfterSpawn = 0f;
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);
    }

    // 씬 초기화
    public void ResetGame()
    {
        enemy_score_change = false;
        human_score_change = false;

        //step 초기화
        resetTimer = 0;
        // Round 초기화&출력
        currentRound = 1; // 현재 라운드 초기화
        ResetGameState();
    }

    public IEnumerator ShowReadyFight()
    {
        // 게임 시작과 움직임 시작!
        humanPlayer_object.GetComponent<Player>().enabled = false;
        enemyPlayer_object.GetComponent<Player>().enabled = false;

        yield return new WaitForSecondsRealtime(3.0f);  // 3초 대기
                                                        // "Ready?" 텍스트 표시
        UI_Base.instance.UpdateGameText("Ready?");
        yield return new WaitForSecondsRealtime(2.0f);  // 2초 대기

        // "Fight!" 텍스트 표시
        UI_Base.instance.UpdateGameText("Fight!");
        yield return new WaitForSecondsRealtime(1.0f);  // 1초 대기

        // "Fight!" 텍스트 사라지게 하려면 빈 문자열로 설정
        UI_Base.instance.UpdateGameText("");

        // 게임 시작과 움직임 시작!
        humanPlayer_object.GetComponent<Player>().enabled = true;
        enemyPlayer_object.GetComponent<Player>().enabled = true;
    }

    public IEnumerator ShowKO()
    {
        // 움직임 멈추기!
        humanPlayer_object.GetComponent<Player>().enabled = false;
        enemyPlayer_object.GetComponent<Player>().enabled = false;

        // "KO!" 텍스트 표시
        UI_Base.instance.UpdateGameText("KO!");
        yield return new WaitForSecondsRealtime(2.0f);  // 1초 대기

        // 텍스트 사라지게
        UI_Base.instance.UpdateGameText("");

    }
    public IEnumerator ShowEnd()
    {
        // 움직임 멈추기!
        humanPlayer_object.GetComponent<Player>().enabled = false;
        enemyPlayer_object.GetComponent<Player>().enabled = false;

        // Player1이 이기면
        if (humanScore > enemyScore)
        {
            // "Player1 Win!" 텍스트 표시
            UI_Base.instance.UpdateGameText("Player1 Win!");
        }
        else // Player2가 이기면
        {
            // "Player2 Win!" 텍스트 표시
            UI_Base.instance.UpdateGameText("Player2 Win!");
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2초 대기

        // 텍스트 사라지게
        UI_Base.instance.UpdateGameText("");

        Debug.Log("All rounds completed!");
    }
}
