using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FSM
{
    private State _currentState;

    public FSM(State initState)
    {
        _currentState = initState;
        ChangeState(_currentState);
    }

    public void ChangeState(State nextState)
    {
        if (nextState == _currentState)
            return;

        if (_currentState != null)
        {
            _currentState.OnStateEnd();
        }

        _currentState = nextState;
        _currentState.OnStateStart();
    }
    public void UpdateState()
    {
        _currentState.OnStateUpdate();
    }
}
