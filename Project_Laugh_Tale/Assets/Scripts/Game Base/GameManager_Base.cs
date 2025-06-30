using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Base : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager_Base instance;

    public GameObject humanPlayerGameObject;
    public GameObject enemyPlayerGameObject;

    // Player 객체 참조 변수
    public HumanPlayer_Base humanPlayer;
    public EnemyPlayer_Base enemyPlayer;

    PlayerData.PlayerType firstHitPlayerType;

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
        ResetGame();
    }

    // 플레이어 점수 올리는 메서드
    public void UpdateScore(PlayerData playerData)
    {
        PlayerData.PlayerType attackerType = playerData.playerType;
        if (attackerType == PlayerData.PlayerType.Human)
        {
            enemyScore++;
        }
        else if (attackerType == PlayerData.PlayerType.Enemy)
        {
            humanScore++;
        }
    }

    void Update()
    {
        // 게임 시작! (gameStarted == True)
        if (!gameStarted) //자동으로 시작
        {
            gameStarted = true;

            StartCoroutine(ShowReadyFight());
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
        // ESC로 로비로 이동하기 (멀티 나감)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Lobby_AI");
        }
    }
    public void DiscountPower()
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

    //hit 판정을 하는 함수
    public void Hit_Handler(PlayerData playerdata)
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

        NextRound();
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
            StartCoroutine(ShowKO());
            StartCoroutine(ShowEnd());
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
        // Score 초기화
        humanScore = 0;
        enemyScore = 0;

        // Round 초기화&출력
        currentRound = 1; // 현재 라운드 초기화

        ResetGameState();
    }

    public IEnumerator ShowReadyFight()
    {
        // 게임 시작과 움직임 시작!
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = false;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = false;

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
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = true;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = true;
    }

    public IEnumerator ShowKO()
    {
        // 움직임 멈추기!
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = false;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = false;

        // "KO!" 텍스트 표시
        UI_Base.instance.UpdateGameText("KO!");
        yield return new WaitForSecondsRealtime(2.0f);  // 1초 대기

        // 텍스트 사라지게
        UI_Base.instance.UpdateGameText("");

    }
    public IEnumerator ShowEnd()
    {
        // 움직임 멈추기!
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = false;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = false;

        // Player1이 이기면
        if (GetHumanScore() > GetEnemyScore())
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
