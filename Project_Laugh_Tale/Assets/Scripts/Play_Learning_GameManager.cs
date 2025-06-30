using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;

public class Play_Learning_GameManager : GameManager
{
    // Player ��ü ���� ����
    public GameObject humanPlayer_object;
    public GameObject enemyPlayer_object;

    // Player ��ü ���� ����
    public Player humanPlayer;
    public Player enemyPlayer;

    //Player Agent ����
    public Agent enemy_Agent;

    //step ��� ��
    private int resetTimer;
    public int MaxEnvironmentSteps;

    // Player �浹 ����
    public enum PlayerSequence { FirstPlayer, SecondPlayer }
    public PlayerSequence playerSequence;
    private PlayerData.PlayerType firstHitPlayerType;

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

    //Score change ���
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

    // �÷��̾� �浹(���º�) �б� ����
    public bool isProcessingCollision = false; // �浹 ó�� ������ ����
    public const float delayThreshold = 0.1f;  // 100ms ������ (���� �浹 ���� Ȯ��)
    public bool isTwo = false; // �ι�° �浹 �߻� ����

    // Round ��ȯ ����
    public int currentRound; // ���� ����
    public bool isRoundTransitioning = false; // ���� �̵� ������ ����

    void Start()
    {
        Debug.Log("Start");

        ResetGame();
    }

    // �÷��̾� ���� �ø��� �޼���
    public void UpdateScore(PlayerData playerData)
    {
        PlayerData.PlayerType attackerType = playerData.playerType;
        if (attackerType == PlayerData.PlayerType.Human || attackerType == PlayerData.PlayerType.Human_AI)
        {
            //�޸��� ���� ����
            enemy_Agent.AddReward(10f);
            enemy_score_change = true;
            enemyScore++;
        }
        else if (attackerType == PlayerData.PlayerType.Enemy || attackerType == PlayerData.PlayerType.Enemy_AI)
        {
            //���� ���� ����
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
                StartCoroutine(ShowReadyFight());
            }
            else
            {
                check_distance();
                check_dir();
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

    private void check_distance()
    {
        float dist = Vector3.Distance(humanPlayer.transform.position, enemyPlayer.transform.position);
        //���� ���� ���� ������ �� ����
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
        Gizmos.color = Color.red;
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
    public override void Hit_Handler(PlayerData playerdata)
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
        //UI.instance.playerSequence = (UI.PlayerSequence)firstPlayerType; // ù��° �浹 �÷��̾ human���� enemy���� ����

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

        NextRound(); // UI�� NextRound ȣ��
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
        enemy_score_change = false;
        human_score_change = false;

        //step �ʱ�ȭ
        resetTimer = 0;
        // Round �ʱ�ȭ&���
        currentRound = 1; // ���� ���� �ʱ�ȭ
        ResetGameState();
    }

    public IEnumerator ShowReadyFight()
    {
        // ���� ���۰� ������ ����!
        humanPlayer_object.GetComponent<Player>().enabled = false;
        enemyPlayer_object.GetComponent<Player>().enabled = false;

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
        humanPlayer_object.GetComponent<Player>().enabled = true;
        enemyPlayer_object.GetComponent<Player>().enabled = true;
    }

    public IEnumerator ShowKO()
    {
        // ������ ���߱�!
        humanPlayer_object.GetComponent<Player>().enabled = false;
        enemyPlayer_object.GetComponent<Player>().enabled = false;

        // "KO!" �ؽ�Ʈ ǥ��
        UI_Base.instance.UpdateGameText("KO!");
        yield return new WaitForSecondsRealtime(2.0f);  // 1�� ���

        // �ؽ�Ʈ �������
        UI_Base.instance.UpdateGameText("");

    }
    public IEnumerator ShowEnd()
    {
        // ������ ���߱�!
        humanPlayer_object.GetComponent<Player>().enabled = false;
        enemyPlayer_object.GetComponent<Player>().enabled = false;

        // Player1�� �̱��
        if (humanScore > enemyScore)
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
