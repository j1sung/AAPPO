using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class State
{
    protected Bot _bot;
    protected State(Bot bot)
    {
        _bot = bot;
    }

    public abstract void OnStateStart();
    public abstract void OnStateUpdate();
    public abstract void OnStateEnd();
}