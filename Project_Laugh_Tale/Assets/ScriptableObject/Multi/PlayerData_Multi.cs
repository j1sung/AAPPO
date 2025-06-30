using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data Multi", menuName = "Scriptable Object/Player Data Multi", order = int.MaxValue)]
public class PlayerData_Multi : ScriptableObject
{
    //Player Type
    public enum PlayerType { Player1, Player2 }
    [SerializeField] private PlayerType playertype;
    public PlayerType playerType { get { return playertype; }}

    //Player ���� ����
    [SerializeField] private bool ispower; //power ȹ�� ����
    public bool isPower { get { return ispower; } set { ispower = value; } }

    [SerializeField] private Vector2 initialposition;   //Player�� �ʱ� ��ġ
    public Vector2 initialPosition { get { return initialposition; } set { initialposition = value; } }

    [SerializeField] private Quaternion initialrotation;    //Player�� �ʱ� ȸ��
    public Quaternion initialRotation { get { return initialrotation; } set { initialrotation = value; } }

    [SerializeField] private float playerspeed;             //player�� �ӵ�
    public float playerSpeed {  get { return playerspeed; } set { playerspeed = value; } }

    [SerializeField] private float dash_speed;              //Dash�� �ӵ�
    public float Dash_speed {  get { return dash_speed; } set { dash_speed = value; } }

    [SerializeField] private bool ondash;                  //Dash ���ΰ�
    public bool onDash { get { return ondash; } set { ondash = value; } }
    

}
