using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Learning_GameManager_Legacy : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static Learning_GameManager_Legacy instance;

    // Player 객체 참조 변수
    public HumanPlayer humanPlayer;
    public EnemyPlayer enemyPlayer;

    //Player Agent 참조
    public Player_Agent human_Agent;
    public Player_Agent enemy_Agent;

    //step 기록 용
    private int resetTimer;
    public int MaxEnvironmentSteps;

    // Player 충돌 순서
    public enum PlayerSequence { FirstPlayer, SecondPlayer }
    public PlayerSequence playerSequence;

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

    public int GetHumanScore()
    {
        return humanScore;
    }
    public int GetEnemyScore()
    {
        return enemyScore;
    }

    // 플레이어 충돌(무승부) 분기 관련
    public bool isProcessingCollision = false; // 충돌 처리 중인지 여부
    public const float delayThreshold = 0.1f;  // 100ms 딜레이 (동시 충돌 여부 확인)
    public bool isTwo = false; // 두번째 충돌 발생 여부

    // Round 전환 관련
    public int currentRound; // 현재 라운드
    public bool isRoundTransitioning = false; // 라운드 이동 중인지 여부

    // 게임 상태 관련
    private bool gameStarted; // 게임이 시작됐는지 여부 확인하는 변수

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            Debug.Log("새로운 UI 인스턴스가 생성되었습니다.");
        }
    }

    void Start()
    {
        Debug.Log("Start");

        ResetGame();
    }

    // 플레이어 점수 올리는 메서드
    public void UpdateScore(PlayerData playerData)
    {
        PlayerData.PlayerType attackerType = playerData.playerType;
        if (attackerType == PlayerData.PlayerType.Human)
        {
            human_Agent.AddReward(0.5f);
            humanScore++;
        }
        else if (attackerType == PlayerData.PlayerType.Enemy)
        {
            enemy_Agent.AddReward(0.5f);
            enemyScore++;
        }
    }

    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            human_Agent.EpisodeInterrupted();
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

            }
            else
            {
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

    public void discountPower()
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
        Gizmos.color = Color.blue;
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
                human_Agent.AddReward(1f);
                enemy_Agent.AddReward(-1f);
            }
            else
            {
                enemy_Agent.AddReward(1f);
                human_Agent.AddReward(-1f);
            }
            ResetGame();
        }
        else // 비긴 상황이거나, 둘 다 3점 미만인 경우 (게임이 아직 끝나지 않음)
        {
            currentRound++;   // 라운드 수 증가
            ResetGameState(); // 게임 상태를 초기화 시킴
        }
        isRoundTransitioning = false;
    }

    // 게임 상태만 초기화 - UI
    public void ResetGameState()
    {
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
        //step 초기화
        resetTimer = 0;
        // Round 초기화&출력
        currentRound = 1; // 현재 라운드 초기화
        ResetGameState();

    }

}
