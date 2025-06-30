using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Scriptable Object/Player Data", order = int.MaxValue)]
public class PlayerData : ScriptableObject
{
    //Player Type
    public enum PlayerType { Human, Enemy, AI, Human_AI, Enemy_AI, FSM }
    [SerializeField] private PlayerType playertype;
    public PlayerType playerType { get { return playertype; }}

    //Player 상태 변수
    [SerializeField] private bool ispower; //power 획득 여부
    public bool isPower { get { return ispower; } set { ispower = value; } }

    [SerializeField] private Vector2 initialposition;   //Player의 초기 위치
    public Vector2 initialPosition { get { return initialposition; } set { initialposition = value; } }

    [SerializeField] private Quaternion initialrotation;    //Player의 초기 회전
    public Quaternion initialRotation { get { return initialrotation; } set { initialrotation = value; } }

    [SerializeField] private float playerspeed;             //player의 속도
    public float playerSpeed {  get { return playerspeed; } set { playerspeed = value; } }

    [SerializeField] private float dash_speed;              //Dash의 속도
    public float Dash_speed {  get { return dash_speed; } set { dash_speed = value; } }

    [SerializeField] private bool ondash;                  //Dash 중인가
    public bool onDash { get { return ondash; } set { ondash = value; } }
    

}
