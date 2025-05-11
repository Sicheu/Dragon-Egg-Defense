using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FSM_DarkDragonBabyState
{
    None,
    FSM_DarkDragonBabyState_Idle,
    FSM_DarkDragonBabyState_Skill,
    FSM_DarkDragonBabyState_OnClicked,
    FSM_DarkDragonBabyState_MoveToPoint
}

public class FSM_DarkDragonBaby : StateMachine<FSM_DarkDragonBabyState>
{
}
