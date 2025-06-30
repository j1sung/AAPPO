using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Photon.Pun;

public class Multi_UI : MonoBehaviourPun
{
    // 싱글톤 인스턴스
    public static Multi_UI instance;

    // UI - 게임 텍스트 출력 변수
    [SerializeField] private Text gameText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text Player1ScoreText;
    [SerializeField] private Text Player2ScoreText;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            Debug.Log("새로운 UI 인스턴스가 생성되었습니다.");
        }
    }

    [PunRPC]
    // 라운드 텍스트를 업데이트하는 함수
    public void UpdateRoundText()
    {
        roundText.text = "Round " + Multi_GameManager.instance.currentRound;
    }

    [PunRPC]
    // 플레이어들 스코어 텍스트를 업데이트하는 함수
    public void UpdateScoreText()
    {
        Player1ScoreText.text = "Score: " + Multi_GameManager.instance.GetPlayer1Score();
        Player2ScoreText.text = "Score: " + Multi_GameManager.instance.GetPlayer2Score();

    }

    [PunRPC]
    public void UpdateGameText(string text)
    {
        gameText.text = text;
    }

}
