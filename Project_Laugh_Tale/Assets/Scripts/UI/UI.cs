using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static UI instance;

    // UI - ���� �ؽ�Ʈ ��� ����
    [SerializeField] private Text gameText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text humanScoreText;
    [SerializeField] private Text enemyScoreText;

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
        roundText.text = "Round " + GameManagerFSM.instance.GetCurrentround();
    }

    // �÷��̾�� ���ھ� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    public void UpdateScoreText()
    {
        humanScoreText.text = "Score: " + GameManagerFSM.instance.GetHumanScore();
        enemyScoreText.text = "Score: " + GameManagerFSM.instance.GetEnemyScore();

    }

    public void UpdateGameText(string text)
    {
        gameText.text = text;
    }

}
