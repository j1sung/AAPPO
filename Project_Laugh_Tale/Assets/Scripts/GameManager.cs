using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager instance;
    // 게임 상태 관련
    public bool gameStarted = false; // 게임이 시작됐는지 여부 확인하는 변수
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            Debug.Log("새로운 UI 인스턴스가 생성되었습니다.");
        }
    }

    public virtual void discountPower() { }
    public virtual void Hit_Handler(PlayerData playerdata) { }

    public virtual int GetHumanScore() { return 0; }
    public virtual int GetEnemyScore() { return 0; }

    public virtual int GetRestTimer() { return 0; }

    public virtual int GetCurrentround() { return 0; }
}
