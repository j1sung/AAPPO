using System.Collections;
using System.Collections.Generic;
using Photon.Pun; // ����Ƽ�� ���� ������Ʈ
using Photon.Realtime; // ���� ���� ���� ���̺귯��
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks // Photon.Pun ����, �ݹ�(�̺�Ʈ,�޽���) ���� ����
{
    private string gameVersion = "1"; // ���� ����
    public enum GameType { Multi, FSM, AI } // ���� Ÿ�� ����
    public static GameType gameType = GameType.Multi;
    public static int gameID = 1; // ���� Ÿ�� ����

    [SerializeField] Text connectionInfoText;  // ��Ʈ��ũ ���� ǥ�� �ؽ�Ʈ
    [SerializeField] Text gameTypeText; // ���� Ÿ�� �ؽ�Ʈ
    [SerializeField] Button joinButton; // �� ���� ��ư
    void Start()
    {
        gameTypeText.text = "#" + gameID;

        if (gameID == 1)
        {
            PhotonNetwork.GameVersion = gameVersion;
            // ���� ������ ������ ���� ���� �õ�
            PhotonNetwork.ConnectUsingSettings();
        }

        // �� ���� ��ư ��Ȱ��ȭ(���� �ȵ�)
        joinButton.interactable = false;

        // ���� �õ� �� �ؽ�Ʈ�� ǥ��
        connectionInfoText.text = "������ ������ ���� ��...";

        if (gameID == 2 || gameID == 3)
        {
            joinButton.interactable = true;
            connectionInfoText.text = "�¶��� : ������ ������ �����!";
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            gameType = GameType.Multi;
            gameID = 1;

            // �ٽ� f1 ������ ������ ���� �� ��ȯ
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            SceneManager.LoadScene("Lobby_Multi");
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            gameType = GameType.FSM;
            gameID = 2;
            SceneManager.LoadScene("Lobby_FSM");
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            gameType = GameType.AI;
            gameID = 3;
            SceneManager.LoadScene("Lobby_AI");
        }
    }

    // ������ ���� ���� ���� �� �ڵ� ����
    public override void OnConnectedToMaster()
    {
        joinButton.interactable = true;
        connectionInfoText.text = "�¶��� : ������ ������ �����!";
    }

    // ������ ���� ���� ���� �� �ڵ� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (gameID == 1)
        {
            joinButton.interactable = false;
            connectionInfoText.text = "�������� : ������ ������ ������� ����\n���� ��õ� ��...";
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    // ���� �� ���� �õ�
    public void Connect()
    {
        joinButton.interactable = false;

        if (gameID == 1)
        {
            if (PhotonNetwork.IsConnected)
            {
                connectionInfoText.text = "�뿡 ����...";
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                connectionInfoText.text = "�������� : ������ ������ ������� ����\n���� ��õ� ��...";
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else if (gameID == 2)
        {
            connectionInfoText.text = "�뿡 ����...";
            connectionInfoText.text = "�� ���� ����";
            SceneManager.LoadScene("DuelFightGame_Base");
        }
        else if (gameID == 3)
        {
            connectionInfoText.text = "�뿡 ����...";
            connectionInfoText.text = "�� ���� ����";
            SceneManager.LoadScene("DuelFightGame_Base");
        }
    }


    // (�� ���� ����) ���� �� ������ ������ ��� �ڵ� ����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ���� ���� ǥ��
        connectionInfoText.text = "�� ���� ����, ���ο� �� ����...";
        // �ִ� 2���� ���� ������ �� �� ����
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        // �� ���� ������ OnJoinedRoom()���� �̵�, ���� ������ �����ڰ� ȣ��Ʈ�� ��
    }


    // �뿡 ���� �Ϸ�� ��� �ڵ� ����
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        connectionInfoText.text = "�� ���� ����";
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("������ Ŭ���̾�Ʈ�� ���� �����մϴ�.");
        }
        // ȣ��Ʈ�� Ŭ���̾�Ʈ ��� ���� ������ �Ѿ, �ڵ� ����ȭ
        PhotonNetwork.LoadLevel("DuelFightGame_Multi");
    }
}
