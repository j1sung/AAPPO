using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Learning_GameManager_Legacy : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static Learning_GameManager_Legacy instance;

    // Player ��ü ���� ����
    public HumanPlayer humanPlayer;
    public EnemyPlayer enemyPlayer;

    //Player Agent ����
    public Player_Agent human_Agent;
    public Player_Agent enemy_Agent;

    //step ��� ��
    private int resetTimer;
    public int MaxEnvironmentSteps;

    // Player �浹 ����
    public enum PlayerSequence { FirstPlayer, SecondPlayer }
    public PlayerSequence playerSequence;

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
        Debug.Log("Start");

        ResetGame();
    }

    // �÷��̾� ���� �ø��� �޼���
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
            //EpisodeInterruoted�� ���Ǽҵ��� �ִ���� ���� �� ����
            //���� ������ ���ܿ� ���� ������ �н��� ���Ե��� �ʱ� ����
            ResetGame();
        }
        else
        {
            // ���� ����! (gameStarted == True)
            if (!gameStarted) //�ڵ����� ����
            {
                gameStarted = true;

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
        }
    }

    public void discountPower()
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
            if(humanScore >= 3)
            {
                //human�� �̱� ��� ���� �ο�
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
        else // ��� ��Ȳ�̰ų�, �� �� 3�� �̸��� ��� (������ ���� ������ ����)
        {
            currentRound++;   // ���� �� ����
            ResetGameState(); // ���� ���¸� �ʱ�ȭ ��Ŵ
        }
        isRoundTransitioning = false;
    }

    // ���� ���¸� �ʱ�ȭ - UI
    public void ResetGameState()
    {
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
        //step �ʱ�ȭ
        resetTimer = 0;
        // Round �ʱ�ȭ&���
        currentRound = 1; // ���� ���� �ʱ�ȭ
        ResetGameState();

    }

}
