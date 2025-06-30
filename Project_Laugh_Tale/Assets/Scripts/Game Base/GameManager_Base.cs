using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Base : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static GameManager_Base instance;

    public GameObject humanPlayerGameObject;
    public GameObject enemyPlayerGameObject;

    // Player ��ü ���� ����
    public HumanPlayer_Base humanPlayer;
    public EnemyPlayer_Base enemyPlayer;

    PlayerData.PlayerType firstHitPlayerType;

    // UI - Power ������ ��� ����
    public GameObject PowerPrefab;  // Power ������
    [SerializeField] private int powerNumber = 7;  // Power 7�� ����
    public Camera mainCamera;    // ī�޶� ����
    private int Power_cnt = 0;  //������ Power ����
    public float spawnRateMin = 0.5f;   //���� �ּ� �ֱ�
    public float spawnRateMax = 3.0f;   //���� �ִ� �ֽ�
    private float timeAfterSpawn = 0.0f; //���� ���� ��� �ð�
    private float spawnRate;    //���� �ֱ�
    public float spawnDistance = 1.5f;  //������ ���ϴ� ����

    // Score ����
    private int humanScore = 0; // humanPlayer Score ����
    private int enemyScore = 0; // enemyPlayer Score ����
    public int GetHumanScore()
    {
        return humanScore;
    }
    public int GetEnemyScore()
    {
        return enemyScore;
    }

    // �÷��̾� �浹(���º�) �б� ����
    public bool isProcessingCollision = false; // �浹 ó�� ������ ����
    public const float delayThreshold = 0.1f;  // 100ms ������ (���� �浹 ���� Ȯ��)
    public bool isTwo = false; // �ι�° �浹 �߻� ����

    // Round ��ȯ ����
    public int currentRound; // ���� ����
    public bool isRoundTransitioning = false; // ���� �̵� ������ ����

    // ���� ���� ����
    private bool gameStarted; // ������ ���۵ƴ��� ���� Ȯ���ϴ� ����

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            Debug.Log("���ο� UI �ν��Ͻ��� �����Ǿ����ϴ�.");
        }
    }
    void Start()
    {
        ResetGame();
    }

    // �÷��̾� ���� �ø��� �޼���
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
        // ���� ����! (gameStarted == True)
        if (!gameStarted) //�ڵ����� ����
        {
            gameStarted = true;

            StartCoroutine(ShowReadyFight());
        }
        else
        {
            //������ �Ŀ� ���� �� �ð��� �󸶳� �귶���� üũ
            timeAfterSpawn += Time.deltaTime;

            //���� �ð��� ���� ��, ���� �Ŀ� ������ ������ �Ŀ� �ٽ� �����
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
        // ESC�� �κ�� �̵��ϱ� (��Ƽ ����)
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

    // Power ������ ȭ�� ����!
    void SpawnPowers()
    {
        Vector2 mapCenter = new Vector2(0, 0);  // �� �߽�
        float mapRadius = 3.5f;                   // �� ������
        // ���� �� ������ ���� ��ġ�� ����
        Vector2 powerPosition = GetRandomPositionInCircle(mapCenter, mapRadius);

        Instantiate(PowerPrefab, powerPosition, Quaternion.identity);
        Power_cnt++;
    }

    private Vector2 GetRandomPositionInCircle(Vector2 center, float radius)
    {
        Vector2 spawnPosition;
        //human�� enemy ������ �Ÿ��� �����Ͽ� ���� �Ÿ� ���̶�� �ٽ� ������ǥ�� ����
        do
        {
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
        // "Power" �±׸� ���� ������Ʈ�� ��� ã�Ƽ� ����
        GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
        foreach (GameObject power in existingPower)
        {
            Destroy(power);
        }
        Power_cnt = 0;
    }

    //hit ������ �ϴ� �Լ�
    public void Hit_Handler(PlayerData playerdata)
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
            firstHitPlayerType = playerdata.playerType;
            isProcessingCollision = true; // �浹 ó������ true ����
            isTwo = false; // �� ��° �浹 ���� �ʱ�ȭ

            StartCoroutine(HandleCollisionWithDelay(playerdata));
        }
        else // �̹� �浹�� ó�� ���̸� �ι�° �浹 ó��
        {
            if (firstHitPlayerType == playerdata.playerType)
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
    private IEnumerator HandleCollisionWithDelay(PlayerData firstPlayerdata)
    {
        
        // ������ �ð� ���� ����ϸ� �� ��° �浹 ���
        yield return new WaitForSeconds(delayThreshold);

        if (isTwo == true)
        {
            UpdateScore(humanPlayer.playerData);
            UpdateScore(enemyPlayer.playerData);
            Debug.Log("���ÿ� �浹�� �Ͼ���ϴ�!");
        }
        else
        {
            UpdateScore(firstPlayerdata);
            Debug.Log("�ѹ��� �浹�� �Ͼ���ϴ�!");
        }

        //�浹 �ʱ�ȭ
        isProcessingCollision = false;
        isTwo = false;

        NextRound();
    }

    // �� ���尡 ���� �� ȣ���Ͽ� ���� ���带 ���� �Լ�
    public void NextRound()  // ���ݿ� ������ �÷��̾��� ������ 1�� �ø��� ȣ���
    {
        isRoundTransitioning = true;
        isProcessingCollision = false;

        // "P_Attack" �±׸� ���� ��� ���� ������Ʈ�� ã�� ����
        GameObject[] p_AttackTag = GameObject.FindGameObjectsWithTag("P_Attack");

        foreach (GameObject obj in p_AttackTag)
        {
            Destroy(obj); // �� ������Ʈ�� ����
        }
        ClearPower();

        // �� �� �ϳ��� ���� 3���� �����߰�, ������ �ٸ��� �̱� ��Ȳ (���� ����)
        if ((humanScore >= 3 || enemyScore >= 3) && (humanScore != enemyScore))
        {
            StartCoroutine(ShowKO());
            StartCoroutine(ShowEnd());
            ResetGame();
        }
        else // ��� ��Ȳ�̰ų�, �� �� 3�� �̸��� ��� (������ ���� ������ ����)
        {
            currentRound++;   // ���� �� ����
            StartCoroutine(ShowKO());
            ResetGameState(); // ���� ���¸� �ʱ�ȭ ��Ŵ
        }
        isRoundTransitioning = false;
    }

    // ���� ���¸� �ʱ�ȭ - UI
    public void ResetGameState()
    {
        UI_Base.instance.UpdateRoundText();
        UI_Base.instance.UpdateScoreText();

        // �÷��̾� ��ġ �ʱ�ȭ
        humanPlayer.restPlayerPosition();
        enemyPlayer.restPlayerPosition();

        gameStarted = false;
        timeAfterSpawn = 0f;
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);
    }

    // �� �ʱ�ȭ
    public void ResetGame()
    {
        // Score �ʱ�ȭ
        humanScore = 0;
        enemyScore = 0;

        // Round �ʱ�ȭ&���
        currentRound = 1; // ���� ���� �ʱ�ȭ

        ResetGameState();
    }

    public IEnumerator ShowReadyFight()
    {
        // ���� ���۰� ������ ����!
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = false;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = false;

        yield return new WaitForSecondsRealtime(3.0f);  // 3�� ���
                                                        // "Ready?" �ؽ�Ʈ ǥ��
        UI_Base.instance.UpdateGameText("Ready?");
        yield return new WaitForSecondsRealtime(2.0f);  // 2�� ���

        // "Fight!" �ؽ�Ʈ ǥ��
        UI_Base.instance.UpdateGameText("Fight!");
        yield return new WaitForSecondsRealtime(1.0f);  // 1�� ���

        // "Fight!" �ؽ�Ʈ ������� �Ϸ��� �� ���ڿ��� ����
        UI_Base.instance.UpdateGameText("");

        // ���� ���۰� ������ ����!
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = true;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = true;
    }

    public IEnumerator ShowKO()
    {
        // ������ ���߱�!
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = false;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = false;

        // "KO!" �ؽ�Ʈ ǥ��
        UI_Base.instance.UpdateGameText("KO!");
        yield return new WaitForSecondsRealtime(2.0f);  // 1�� ���

        // �ؽ�Ʈ �������
        UI_Base.instance.UpdateGameText("");

    }
    public IEnumerator ShowEnd()
    {
        // ������ ���߱�!
        humanPlayerGameObject.GetComponent<Player_Base>().enabled = false;
        enemyPlayerGameObject.GetComponent<Player_Base>().enabled = false;

        // Player1�� �̱��
        if (GetHumanScore() > GetEnemyScore())
        {
            // "Player1 Win!" �ؽ�Ʈ ǥ��
            UI_Base.instance.UpdateGameText("Player1 Win!");
        }
        else // Player2�� �̱��
        {
            // "Player2 Win!" �ؽ�Ʈ ǥ��
            UI_Base.instance.UpdateGameText("Player2 Win!");
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2�� ���

        // �ؽ�Ʈ �������
        UI_Base.instance.UpdateGameText("");

        Debug.Log("All rounds completed!");
    }

}
