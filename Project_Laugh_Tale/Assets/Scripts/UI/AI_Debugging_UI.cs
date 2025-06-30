using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class AI_Debugging_UI : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static AI_Debugging_UI instance;

    // UI - 게임 텍스트 출력 변수
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
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            Debug.Log("새로운 UI 인스턴스가 생성되었습니다.");
        }
    }

    // 라운드 텍스트를 업데이트하는 함수
    public void UpdateRoundText()
    {
        roundText.text = "Round " + GameManager.instance.GetCurrentround();
    }

    // 플레이어들 스코어 텍스트를 업데이트하는 함수
    public void UpdateScoreText()
    {
        humanScoreText.text = "Score: " + GameManager.instance.GetHumanScore();
        enemyScoreText.text = "Score: " + GameManager.instance.GetEnemyScore();

    }

    // 플레이어들 step 텍스트를 업데이트하는 함수
    public void UpdateStepText()
    {
        stepText.text = "Step: " + GameManager.instance.GetRestTimer();

    }

    // Human Degree 텍스트를 업데이트하는 함수
    public void UpdateHuman_DegreeText(bool human_match, float cal_degree)
    {
        Human_DegreeText.text = "human_Degree_match: " + human_match + "\n" +
                                     "Human_cal_Degree: " + cal_degree;

    }

    // Enemy Degree 텍스트를 업데이트하는 함수
    public void UpdateEnemy_DegreeText(bool enemy_match,float cal_degree)
    {
        Enemy_DegreeText.text = "Enemy_Degree_match: " + enemy_match + "\n" +
                                    "Enemy_cal_Degree: " + cal_degree;
                                    

    }

    //Dist Reward 텍스트를 업데이트하는 함수
    public void UpdateDist_RewardText(float reward)
    {
        Dist_RewardText.text = "Dist_Reward: " + reward;


    }

    public void Update_Power_position_Text(float x, float y)
    {
        Power_position_Text.text = "position" + x + ", " + y;
    }

}
