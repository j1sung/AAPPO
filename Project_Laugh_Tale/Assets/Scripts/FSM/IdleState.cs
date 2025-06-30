using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class IdleState : State
{
    public IdleState(Bot bot) : base(bot)
    {
        _bot = bot;
    }


    public override void OnStateStart()
    {
        Debug.Log("===== Start Idle =====");
    }

    public override void OnStateUpdate()
    {
    }

    public override void OnStateEnd()
    {
        Debug.Log("===== End Idle =====");
    }
}
