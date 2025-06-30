using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Photon.Pun;

public class Multi_UI : MonoBehaviourPun
{
    // �̱��� �ν��Ͻ�
    public static Multi_UI instance;

    // UI - ���� �ؽ�Ʈ ��� ����
    [SerializeField] private Text gameText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text Player1ScoreText;
    [SerializeField] private Text Player2ScoreText;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            Debug.Log("���ο� UI �ν��Ͻ��� �����Ǿ����ϴ�.");
        }
    }

    [PunRPC]
    // ���� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateRoundText()
    {
        roundText.text = "Round " + Multi_GameManager.instance.currentRound;
    }

    [PunRPC]
    // �÷��̾�� ���ھ� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
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
