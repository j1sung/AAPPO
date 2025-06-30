using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static GameManager instance;
    // ���� ���� ����
    public bool gameStarted = false; // ������ ���۵ƴ��� ���� Ȯ���ϴ� ����
    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            Debug.Log("���ο� UI �ν��Ͻ��� �����Ǿ����ϴ�.");
        }
    }

    public virtual void discountPower() { }
    public virtual void Hit_Handler(PlayerData playerdata) { }

    public virtual int GetHumanScore() { return 0; }
    public virtual int GetEnemyScore() { return 0; }

    public virtual int GetRestTimer() { return 0; }

    public virtual int GetCurrentround() { return 0; }
}
