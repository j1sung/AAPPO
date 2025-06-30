using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UI_Base : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UI_Base instance;

    // UI - 게임 텍스트 출력 변수
    [SerializeField] private Text gameText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text humanScoreText;
    [SerializeField] private Text enemyScoreText;

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

    public void UpdateGameText(string text)
    {
        gameText.text = text;
    }
}
