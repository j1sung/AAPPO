using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class AI_Debugging_UI : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static AI_Debugging_UI instance;

    // UI - ���� �ؽ�Ʈ ��� ����
    [SerializeField] private Text roundText;
    [SerializeField] private Text humanScoreText;
    [SerializeField] private Text enemyScoreText;
    [SerializeField] private Text stepText;
    [SerializeField] private Text Human_DegreeText;
    [SerializeField] private Text Enemy_DegreeText;
    [SerializeField] private Text Dist_RewardText;
    [SerializeField] private Text Power_position_Text;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            Debug.Log("���ο� UI �ν��Ͻ��� �����Ǿ����ϴ�.");
        }
    }

    // ���� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateRoundText()
    {
        roundText.text = "Round " + GameManager.instance.GetCurrentround();
    }

    // �÷��̾�� ���ھ� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateScoreText()
    {
        humanScoreText.text = "Score: " + GameManager.instance.GetHumanScore();
        enemyScoreText.text = "Score: " + GameManager.instance.GetEnemyScore();

    }

    // �÷��̾�� step �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateStepText()
    {
        stepText.text = "Step: " + GameManager.instance.GetRestTimer();

    }

    // Human Degree �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateHuman_DegreeText(bool human_match, float cal_degree)
    {
        Human_DegreeText.text = "human_Degree_match: " + human_match + "\n" +
                                     "Human_cal_Degree: " + cal_degree;

    }

    // Enemy Degree �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateEnemy_DegreeText(bool enemy_match,float cal_degree)
    {
        Enemy_DegreeText.text = "Enemy_Degree_match: " + enemy_match + "\n" +
                                    "Enemy_cal_Degree: " + cal_degree;
                                    

    }

    //Dist Reward �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateDist_RewardText(float reward)
    {
        Dist_RewardText.text = "Dist_Reward: " + reward;


    }

    public void Update_Power_position_Text(float x, float y)
    {
        Power_position_Text.text = "position" + x + ", " + y;
    }

}
