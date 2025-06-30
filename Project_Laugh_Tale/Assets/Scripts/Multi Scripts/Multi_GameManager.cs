using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Multi_GameManager : MonoBehaviourPunCallbacks
{
    // �̱��� �ν��Ͻ�
    public static Multi_GameManager instance;

    public GameObject player1GameObject;
    public GameObject player2GameObject;

    // Player ��ü ���� ����
    public Multi_Player1 player1;
    public Multi_Player2 player2;

    PlayerData_Multi.PlayerType firstHitPlayerType;

    // UI - Power ������ ��� ����
    [SerializeField] private int powerNumber = 7;  // Power 7�� ����
    public Camera mainCamera;    // ī�޶� ����
    private int Power_cnt = 0;  //������ Power ����
    public float spawnRateMin = 0.5f;   //���� �ּ� �ֱ�
    public float spawnRateMax = 3.0f;   //���� �ִ� �ֽ�
    private float timeAfterSpawn = 0.0f; //���� ���� ��� �ð�
    private float spawnRate;    //���� �ֱ�
    public float spawnDistance = 1.5f;  //������ ���ϴ� ����

    // Score ����
    private int player1Score = 0; // humanPlayer Score ����
    private int player2Score = 0; // enemyPlayer Score ����

    public int GetPlayer1Score()
    {
        return player1Score;
    }
    public int GetPlayer2Score()
    {
        return player2Score;
    }

    // �÷��̾� �浹(���º�) �б� ����
    public bool isProcessingCollision = false; // �浹 ó�� ������ ����
    public const float delayThreshold = 0.1f;  // 100ms ������ (���� �浹 ���� Ȯ��)
    public bool isTwo = false; // �ι�° �浹 �߻� ����

    // Round ��ȯ ����
    public int currentRound; // ���� ����
    public bool isRoundTransitioning = false; // ���� �̵� ������ ����

    // ���� ���� ����
    public bool gameStarted; // ������ ���۵ƴ��� ���� Ȯ���ϴ� ����

    // Player ���� ��ġ
    public Vector3 player1Spawn = new Vector3(0, -3.18f, 0); // 1P ���� ��ġ
    public Vector3 player2Spawn = new Vector3(0, 3.18f, 0);  // 2P ���� ��ġ

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            Debug.Log("���ο� UI �ν��Ͻ��� �����Ǿ����ϴ�.");
        }

        // 1P(ȣ��Ʈ)�� HumanPlayer �Ҵ�
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("������ Ŭ���̾�Ʈ!");
            // ù ��° Ŭ���̾�Ʈ�� HumanPlayer�� ����
            GameObject playerObject = PhotonNetwork.Instantiate("Multi_Player1", player1Spawn, Quaternion.identity);
            int player1ViewID = playerObject.GetComponent<PhotonView>().ViewID;
            photonView.RPC("AssignPlayer1", RpcTarget.AllBuffered, player1ViewID);
        }
        // 2P(Ŭ���̾�Ʈ)�� EnemyPlayer �Ҵ�
        else
        {
            // �� ��° Ŭ���̾�Ʈ�� EnemyPlayer�� ����
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
        // Round �ʱ�ȭ
        currentRound = 1; // ���� ���� �ʱ�ȭ

        // Player Score & Round ���
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateRoundText", RpcTarget.AllBuffered);
            Multi_UI.instance.photonView.RPC("UpdateScoreText", RpcTarget.AllBuffered);
        }

        // ���� �ʱ� ����
        gameStarted = false; // ���� ���� ����

        timeAfterSpawn = 0f;
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);
    }

    // �÷��̾� ���� �ø��� �޼���
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
        StartCoroutine(ShowReadyFight()); // ���� ���� �ȳ�
    }

    void Update()
    {
        // 3�� �ڿ� ���� ����! (gameStarted == True)
        if (!gameStarted && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("GameStart", RpcTarget.All);
            }
        }
        else if (gameStarted)
        {
            //������ �Ŀ� ���� �� �ð��� �󸶳� �귶���� üũ
            timeAfterSpawn += Time.deltaTime;

            //���� �ð��� ���� ��, ���� �Ŀ� ������ ������ �Ŀ� �ٽ� �����
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
        // ESC�� �κ�� �̵��ϱ� (��Ƽ ����)
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

    // GetRandomPositionInCircle �޼���� ��ǥ ���� �� RPC�� ����
    [PunRPC]
    public void SpawnPowers()
    {
        if (PhotonNetwork.IsMasterClient) // ������ Ŭ���̾�Ʈ������ ����
        {
            Vector2 mapCenter = new Vector2(0, 0);  // �� �߽�
            float mapRadius = 3.5f;                   // �� ������
            // ���� �� ������ ���� ��ġ�� ����
            Vector2 powerPosition = GetRandomPositionInCircle(mapCenter, mapRadius);

            PhotonNetwork.Instantiate("Multi_Power", powerPosition, Quaternion.identity);
            Power_cnt++;
        }
    }

    private Vector2 GetRandomPositionInCircle(Vector2 center, float radius)
    {
        Vector2 spawnPosition;
        //human�� enemy ������ �Ÿ��� �����Ͽ� ���� �Ÿ� ���̶�� �ٽ� ������ǥ�� ����
        do
        {
            Debug.Log("player1: " + player1.transform.position);
            Debug.Log("player2: " + player2.transform.position);

            // ������ ���� (0 ~ 360��) ����
            float angle = Random.Range(0f, Mathf.PI * 2);

            // ������ �Ÿ� (0 ~ radius) ����
            float distance = Random.Range(0f, radius);

            // Polar Coordinates�� Cartesian Coordinates�� ��ȯ
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
            // "Power" �±׸� ���� ������Ʈ�� ��� ã�Ƽ� ����
            GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
            foreach (GameObject power in existingPower)
            {
                PhotonNetwork.Destroy(power);
            }
        }
        Power_cnt = 0;
    }

    // �浹 ó�� ������
    [PunRPC]
    public void Hit_Handler(PlayerData_Multi.PlayerType playerType)
    {
        // ���̳� �˱� �浹 ó��
        // ���� ���� �̵� ���̶�� �浹 ����
        if (isRoundTransitioning)
        {
            Debug.Log("���� �̵� ���̹Ƿ� �浹 ����");
            return;
        }

        

        // ù ��° �浹�� ����ϰ� ó�� ����
        if (!isProcessingCollision) // isProcessingCollision == false �϶�
        {
            firstHitPlayerType = playerType;
            isProcessingCollision = true; // �浹 ó������ true ����
            isTwo = false; // �� ��° �浹 ���� �ʱ�ȭ

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(HandleCollisionWithDelay(playerType));
            }
        }
        else // �̹� �浹�� ó�� ���̸� �ι�° �浹 ó��
        {
            if (firstHitPlayerType == playerType)
            {
                return;
            }
            else
            {
                isTwo = true; // �ι�° �÷��̾� �浹 �ν�
            }
        }
    }

    // �ι�° �浹�� �߻��Ѵٸ� ù��° �浹�� ����ϸ� �ι�° �浹�� ��ٸ��� NextRound()�� ���� ȣ���ϴ� �ڷ�ƾ
    private IEnumerator HandleCollisionWithDelay(PlayerData_Multi.PlayerType playerType)
    {

        // ������ �ð� ���� ����ϸ� �� ��° �浹 ���
        yield return new WaitForSeconds(delayThreshold);

        if (isTwo == true)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("UpdateScore", RpcTarget.All, player1.playerData.playerType); // ���� ���� �÷��̾� ���� ����
                photonView.RPC("UpdateScore", RpcTarget.All, player2.playerData.playerType); // ���� ���� �÷��̾� ���� ����
            }
            Debug.Log("���ÿ� �浹�� �Ͼ���ϴ�!");
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("UpdateScore", RpcTarget.All, playerType); // ���� ���� �÷��̾� ���� ����
            }
            Debug.Log("�ѹ��� �浹�� �Ͼ���ϴ�!");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("NextRound", RpcTarget.All);
        }
    }

    [PunRPC]
    // �� ���尡 ���� �� ȣ���Ͽ� ���� ���带 ���� �Լ�
    public void NextRound()  // ���ݿ� ������ �÷��̾��� ������ 1�� �ø��� ȣ���
    {
        Debug.Log("P1 P: " + player1.getPosition());
        Debug.Log("P1 R: " + player1.getRotation());
        Debug.Log("P2 P: " + player2.getPosition());
        Debug.Log("P2 R: " + player2.getRotation());

        isProcessingCollision = false; // �ٽ� ù��° �浹 ó������ false �ʱ�ȭ
        isTwo = false;                 // �ٽ� �ι�° �浹 ó������ false �ʱ�ȭ

        isRoundTransitioning = true;
        isProcessingCollision = false;

        // "P_Attack" �±׸� ���� ��� ���� ������Ʈ�� ã�� ����
        GameObject[] p_AttackTag = GameObject.FindGameObjectsWithTag("P_Attack");

        foreach (GameObject obj in p_AttackTag)
        {
            PhotonNetwork.Destroy(obj); // �� ������Ʈ�� ����
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // 'Power' �±� ���� ���� ������Ʈ ��� ����
            photonView.RPC("ClearPower", RpcTarget.All);
        }

        // �� �� �ϳ��� ���� 3���� �����߰�, ������ �ٸ��� �̱� ��Ȳ (���� ����)
        if ((player1Score >= 3 || player2Score >= 3) && (player1Score != player2Score))
        {

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("KO", RpcTarget.All);
                photonView.RPC("End", RpcTarget.All);
                photonView.RPC("ResetGame", RpcTarget.All);
            }
        }
        else // ��� ��Ȳ�̰ų�, �� �� 3�� �̸��� ��� (������ ���� ������ ����)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("KO", RpcTarget.All);
                photonView.RPC("ResetGameState", RpcTarget.All); // ���� ���¸� �ʱ�ȭ ��Ŵ
            }
        }
        isRoundTransitioning = false;
    }

    [PunRPC]
    public void KO()
    {
        currentRound++;
        StartCoroutine(ShowKO()); // ���� ���� �ȳ�
    }

    [PunRPC]
    public void End()
    {
        Debug.Log(GetPlayer1Score());
        Debug.Log(GetPlayer2Score());
        StartCoroutine(ShowEnd()); // ���� ���� �ȳ�
    }


    // ���� ���¸� �ʱ�ȭ - UI
    [PunRPC]
    public void ResetGameState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Power �ʱ�ȭ
            player1.photonView.RPC("PowerRingState", RpcTarget.All, false, false);
            player2.photonView.RPC("PowerRingState", RpcTarget.All, false, false);

            // ������ Score, Round �ٽ� ���
            Multi_UI.instance.photonView.RPC("UpdateScoreText", RpcTarget.All);
            Multi_UI.instance.photonView.RPC("UpdateRoundText", RpcTarget.All);
        }

        // �÷��̾� ��ġ&���� �ʱ�ȭ
        player1.restPlayerPosition();
        player2.restPlayerPosition();

        gameStarted = false;
        timeAfterSpawn = 0f;
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);

    }

    [PunRPC]
    // �� �ʱ�ȭ
    public void ResetGame()
    {
        // Score �ʱ�ȭ
        player1Score = 0;
        player2Score = 0;

        // Round �ʱ�ȭ&���
        currentRound = 1; // ���� ���� �ʱ�ȭ

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetGameState", RpcTarget.All);
        }
    }

    public IEnumerator ShowReadyFight()
    {
        // ���� ���۰� ������ ����!
        player1GameObject.GetComponent<Multi_Player>().enabled = false;
        player2GameObject.GetComponent<Multi_Player>().enabled = false;

        yield return new WaitForSecondsRealtime(3.0f);  // 3�� ���
                                                        // "Ready?" �ؽ�Ʈ ǥ��
        if(PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Ready?");
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2�� ���

        // "Fight!" �ؽ�Ʈ ǥ��
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Fight!");
        }
        yield return new WaitForSecondsRealtime(1.0f);  // 1�� ���

        // "Fight!" �ؽ�Ʈ ������� �Ϸ��� �� ���ڿ��� ����
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "");
        }

        // ���� ���۰� ������ ����!
        player1GameObject.GetComponent<Multi_Player>().enabled = true;
        player2GameObject.GetComponent<Multi_Player>().enabled = true;
    }

    public IEnumerator ShowKO()
    {
        // ������ ���߱�!
        player1GameObject.GetComponent<Multi_Player>().enabled = false;
        player2GameObject.GetComponent<Multi_Player>().enabled = false;

        // "KO!" �ؽ�Ʈ ǥ��
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "KO!");
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 1�� ���

        // �ؽ�Ʈ �������
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "");
        }

    }
    public IEnumerator ShowEnd()
    {
        // ������ ���߱�!
        player1GameObject.GetComponent<Multi_Player>().enabled = false;
        player2GameObject.GetComponent<Multi_Player>().enabled = false;

        // Player1�� �̱��
        if (GetPlayer1Score() > GetPlayer2Score())
        {
            // "Player1 Win!" �ؽ�Ʈ ǥ��
            if (PhotonNetwork.IsMasterClient)
            {
                Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Player1 Win!");
            }
        }
        else // Player2�� �̱��
        {
            // "Player2 Win!" �ؽ�Ʈ ǥ��
            if (PhotonNetwork.IsMasterClient)
            {
                Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "Player2 Win!");
            }
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2�� ���

        // �ؽ�Ʈ �������
        if (PhotonNetwork.IsMasterClient)
        {
            Multi_UI.instance.photonView.RPC("UpdateGameText", RpcTarget.All, "");
        }

        Debug.Log("All rounds completed!");
    }

}
