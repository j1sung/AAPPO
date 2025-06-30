using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Ai : MonoBehaviour
{
    private enum AiStateEnum
    {
        IDLE,
        ATTACKING,
        PATROLLING,
        CHASING,
        ITEM,
        DASHING
    }
    private AiStateEnum _state;

    private void Start()
    {
        _state = AiStateEnum.IDLE;
    }

    private void Update()
    {
        switch (_state)
        {
            case AiStateEnum.IDLE:
                break;
            case AiStateEnum.ATTACKING:
                break;
            case AiStateEnum.PATROLLING:
                break;
            case AiStateEnum.CHASING:
                break;
            case AiStateEnum.ITEM:
                break;
            case AiStateEnum.DASHING:
                break;
            default:
                break;
        }
    }
}
