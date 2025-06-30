using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Multi_GameManager : MonoBehaviourPunCallbacks
{
    // 싱글톤 인스턴스
    public static Multi_GameManager instance;

    public GameObject player1GameObject;
    public GameObject player2GameObject;

    // Player 객체 참조 변수
    public Multi_Player1 player1;
    public Multi_Player2 player2;

    PlayerData_Multi.PlayerType firstHitPlayerType;

    // UI - Power 프리펩 출력 관련
    [SerializeField] private int powerNumber = 7;  // Power 7개 생성
    public Camera mainCamera;    // 카메라 참조
    private int Power_cnt = 0;  //생성한 Power 개수
    public float spawnRateMin = 0.5f;   //생성 최소 주기
    public float spawnRateMax = 3.0f;   //생성 최대 주시
    private float timeAfterSpawn = 0.0f; //다음 생성 대기 시간
    private float spawnRate;    //생성 주기
    public float spawnDistance = 1.5f;  //생성을 피하는 범위

    // Score 관련
    private int player1Score = 0; // humanPlayer Score 변수
    private int player2Score = 0; // enemyPlayer Score 변수

    public int GetPlayer1Score()
    {
        return player1Score;
    }
    public int GetPlayer2Score()
    {
        return player2Score;
    }

    // 플레이어 충돌(무승부) 분기 관련
    public bool isProcessingCollision = false; // 충돌 처리 중인지 여부
    public const float delayThreshold = 0.1f;  // 100ms 딜레이 (동시 충돌 여부 확인)
    public bool isTwo = false; // 두번째 충돌 발생 여부

    // Round 전환 관련
    public int currentRound; // 현재 라운드
    public bool isRoundTransitioning = false; // 라운드 이동 중인지 여부

    // 게임 상태 관련
    public bool gameStarted; // 게임이 시작됐는지 여부 확인하는 변수

    // Player 스폰 위치
    public Vector3 player1Spawn = new Vector3(0, -3.18f, 0); // 1P 스폰 위치
    public Vector3 player2Spawn = new Vector3(0, 3.18f, 0);  // 2P 스폰 위치

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            Debug.Log("새로운 UI 인스턴스가 생성되었습니다.");
        }

        // 1P(호스트)는 HumanPlayer 할당
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터 클라이언트!");
            // 첫 번째 클라이언트는 HumanPlayer를 생성
            GameObject playerObject = PhotonNetwork.Instantiate("Multi_Player1", player1Spawn, Quaternion.identity);
            int player1ViewID = playerObject.GetComponent<PhotonView>().ViewID;
            photonView.RPC("AssignPlayer1", RpcTarget.AllBuffered, player1ViewID);
        }
        // 2P(클라이언트)는 EnemyPlayer 할당
        else
        {
            // 두 번째 클라이언트는 EnemyPlayer를 생성
            GameObject playerObject = PhotonNetwork.Instantiate("Multi_Player2", player2Spawn, Quaternion.Euler(0, 0, -180));
            int player2ViewID = playerObject.GetComponent<PhotonView>().ViewID;
            photonView.RPC("AssignPlayer2", RpcTarget.AllBuffered, player2ViewID);
        }
    }

    [PunRPC]
    public void AssignPlayer1(int viewID)
    {
        player1 = PhotonView.Find(viewID).GetComponent<Multi_Player1>();
    }
    [PunRPC]
    public void AssignPlayer2(int viewID)
    {
        player2 = PhotonView.Find(viewID).GetComponent<Multi_Player2>();
    }

    void Start()
    {
        Debug.Log("Start");
        // Round 초기화
        currentRound = 1; // 현재 라운드 초기화

        // Player Score & Round 출력
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateRoundText", RpcTarget.AllBuffered);
            Multi_UI.instance.photonView.RPC("UpdateScoreText", RpcTarget.AllBuffered);
        }

        // 게임 초기 설정
        gameStarted = false; // 게임 시작 유무

        timeAfterSpawn = 0f;
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);
    }

    // 플레이어 점수 올리는 메서드
    [PunRPC]
    public void UpdateScore(PlayerData_Multi.PlayerType playerType)
    {
        if (playerType == PlayerData_Multi.PlayerType.Player1)
        {
            player2Score++;
        }
        else if (playerType == PlayerData_Multi.PlayerType.Player2)
        {
            player1Score++;
        }
    }

    [PunRPC]
    public void GameStart()
    {
        gameStarted = true;
        StartCoroutine(ShowReadyFight()); // 게임 시작 안내
    }

    void Update()
    {
        // 3초 뒤에 게임 시작! (gameStarted == True)
        if (!gameStarted && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("GameStart", RpcTarget.All);
            }
        }
        else if (gameStarted)
        {
            //마지막 파워 생성 후 시간이 얼마나 흘렀는지 체크
            timeAfterSpawn += Time.deltaTime;

            //생성 시간이 지난 후, 아직 파워 개수가 안차면 파워 다시 만들기
            if (timeAfterSpawn > spawnRate && Power_cnt < powerNumber)
            {
                timeAfterSpawn = 0f;
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("SpawnPowers", RpcTarget.All);
                }
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
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    [PunRPC]
    public void DiscountPower()
    {
        if (Power_cnt > 0)
        {
            Power_cnt--;
        }
    }

    // GetRandomPositionInCircle 메서드로 좌표 생성 후 RPC로 전파
    [PunRPC]
    public void SpawnPowers()
    {
        if (PhotonNetwork.IsMasterClient) // 마스터 클라이언트에서만 실행
        {
            Vector2 mapCenter = new Vector2(0, 0);  // 맵 중심
            float mapRadius = 3.5f;                   // 맵 반지름
            // 원형 맵 내부의 랜덤 위치에 생성
            Vector2 powerPosition = GetRandomPositionInCircle(mapCenter, mapRadius);

            PhotonNetwork.Instantiate("Multi_Power", powerPosition, Quaternion.identity);
            Power_cnt++;
        }
    }

    private Vector2 GetRandomPositionInCircle(Vector2 center, float radius)
    {
        Vector2 spawnPosition;
        //human과 enemy 사이의 거리를 측정하여 일정 거리 안이라면 다시 생성좌표를 생성
        do
        {
            Debug.Log("player1: " + player1.transform.position);
            Debug.Log("player2: " + player2.transform.position);

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
        while (Vector2.Distance(player1.transform.position, spawnPosition) <= spawnDistance ||
            Vector2.Distance(player2.transform.position, spawnPosition) <= spawnDistance);
        return spawnPosition;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player1.transform.position, spawnDistance);
        Gizmos.DrawWireSphere(player2.transform.position, spawnDistance);
    }

    [PunRPC]
    void ClearPower()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // "Power" 태그를 가진 오브젝트를 모두 찾아서 삭제
            GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
            foreach (GameObject power in existingPower)
            {
                PhotonNetwork.Destroy(power);
            }
        }
        Power_cnt = 0;
    }

    // 충돌 처리 구현부
    [PunRPC]
    public void Hit_Handler(PlayerData_Multi.PlayerType playerType)
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
            firstHitPlayerType = playerType;
            isProcessingCollision = true; // 충돌 처리여부 true 변경
            isTwo = false; // 두 번째 충돌 여부 초기화

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(HandleCollisionWithDelay(playerType));
            }
        }
        else // 이미 충돌을 처리 중이면 두번째 충돌 처리
        {
            if (firstHitPlayerType == playerType)
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
    private IEnumerator HandleCollisionWithDelay(PlayerData_Multi.PlayerType playerType)
    {

        // 딜레이 시간 동안 대기하며 두 번째 충돌 대기
        yield return new WaitForSeconds(delayThreshold);

        if (isTwo == true)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("UpdateScore", RpcTarget.All, player1.playerData.playerType); // 공격 성공 플레이어 점수 변동
                photonView.RPC("UpdateScore", RpcTarget.All, player2.playerData.playerType); // 공격 성공 플레이어 점수 변동
            }
            Debug.Log("동시에 충돌이 일어났습니다!");
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("UpdateScore", RpcTarget.All, playerType); // 공격 성공 플레이어 점수 변동
            }
            Debug.Log("한번의 충돌이 일어났습니다!");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("NextRound", RpcTarget.All);
        }
    }

    [PunRPC]
    // 한 라운드가 끝날 때 호출하여 다음 라운드를 진행 함수
    public void NextRound()  // 공격에 성공한 플레이어의 점수를 1개 올리고 호출됨
    {
        Debug.Log("P1 P: " + player1.getPosition());
        Debug.Log("P1 R: " + player1.getRotation());
        Debug.Log("P2 P: " + player2.getPosition());
        Debug.Log("P2 R: " + player2.getRotation());

        isProcessingCollision = false; // 다시 첫번째 충돌 처리여부 false 초기화
        isTwo = false;                 // 다시 두번째 충돌 처리여부 false 초기화

        isRoundTransitioning = true;
        isProcessingCollision = false;

        // "P_Attack" 태그를 가진 모든 게임 오브젝트를 찾아 삭제
        GameObject[] p_AttackTag = GameObject.FindGameObjectsWithTag("P_Attack");

        foreach (GameObject obj in p_AttackTag)
        {
            PhotonNetwork.Destroy(obj); // 각 오브젝트를 삭제
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // 'Power' 태그 가진 게임 오브젝트 모두 삭제
            photonView.RPC("ClearPower", RpcTarget.All);
        }

        // 둘 중 하나가 먼저 3점에 도달했고, 점수가 다르면 이긴 상황 (승패 결정)
        if ((player1Score >= 3 || player2Score >= 3) && (player1Score != player2Score))
        {

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("KO", RpcTarget.All);
                photonView.RPC("End", RpcTarget.All);
                photonView.RPC("ResetGame", RpcTarget.All);
            }
        }
        else // 비긴 상황이거나, 둘 다 3점 미만인 경우 (게임이 아직 끝나지 않음)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("KO", RpcTarget.All);
                photonView.RPC("ResetGameState", RpcTarget.All); // 게임 상태를 초기화 시킴
            }
        }
        isRoundTransitioning = false;
    }

    [PunRPC]
    public void KO()
    {
        currentRound++;
        StartCoroutine(ShowKO()); // 게임 시작 안내
    }

    [PunRPC]
    public void End()
    {
        Debug.Log(GetPlayer1Score());
        Debug.Log(GetPlayer2Score());
        StartCoroutine(ShowEnd()); // 게임 시작 안내
    }


    // 게임 상태만 초기화 - UI
    [PunRPC]
    public void ResetGameState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Power 초기화
            player1.photonView.RPC("PowerRingState", RpcTarget.All, false, false);
            player2.photonView.RPC("PowerRingState", RpcTarget.All, false, false);

            // 수정된 Score, Round 다시 출력
            Multi_UI.instance.photonView.RPC("UpdateScoreText", RpcTarget.All);
            Multi_UI.instance.photonView.RPC("UpdateRoundText", RpcTarget.All);
        }

        // 플레이어 위치&방향 초기화
        player1.restPlayerPosition();
        player2.restPlayerPosition();

        gameStarted = false;
        timeAfterSpawn = 0f;
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);

    }

    [PunRPC]
    // 씬 초기화
    public void ResetGame()
    {
        // Score 초기화
        player1Score = 0;
        player2Score = 0;

        // Round 초기화&출력
        currentRound = 1; // 현재 라운드 초기화

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetGameState", RpcTarget.All);
        }
    }

    public IEnumerator ShowReadyFight()
    {
        // 게임 시작과 움직임 시작!
        player1GameObject.GetComponent<Multi_Player>().enabled = false;
        player2GameObject.GetComponent<Multi_Player>().enabled = false;

        yield return new WaitForSecondsRealtime(3.0f);  // 3초 대기
                                                        // "Ready?" 텍스트 표시
        if(PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Ready?");
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2초 대기

        // "Fight!" 텍스트 표시
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Fight!");
        }
        yield return new WaitForSecondsRealtime(1.0f);  // 1초 대기

        // "Fight!" 텍스트 사라지게 하려면 빈 문자열로 설정
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "");
        }

        // 게임 시작과 움직임 시작!
        player1GameObject.GetComponent<Multi_Player>().enabled = true;
        player2GameObject.GetComponent<Multi_Player>().enabled = true;
    }

    public IEnumerator ShowKO()
    {
        // 움직임 멈추기!
        player1GameObject.GetComponent<Multi_Player>().enabled = false;
        player2GameObject.GetComponent<Multi_Player>().enabled = false;

        // "KO!" 텍스트 표시
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "KO!");
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 1초 대기

        // 텍스트 사라지게
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "");
        }

    }
    public IEnumerator ShowEnd()
    {
        // 움직임 멈추기!
        player1GameObject.GetComponent<Multi_Player>().enabled = false;
        player2GameObject.GetComponent<Multi_Player>().enabled = false;

        // Player1이 이기면
        if (GetPlayer1Score() > GetPlayer2Score())
        {
            // "Player1 Win!" 텍스트 표시
            if (PhotonNetwork.IsMasterClient)
            {
                Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Player1 Win!");
            }
        }
        else // Player2가 이기면
        {
            // "Player2 Win!" 텍스트 표시
            if (PhotonNetwork.IsMasterClient)
            {
                Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Player2 Win!");
            }
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2초 대기

        // 텍스트 사라지게
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "");
        }

        Debug.Log("All rounds completed!");
    }

}
