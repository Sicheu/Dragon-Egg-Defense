using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 캐릭터1 StateMachine
public enum FSM_Character1State
{
    None, 
    FSM_Character1State_Idle,
    FSM_Character1State_MoveToDestination,
    FSM_Character1State_Skill1
}

public class FSM_Character1 : StateMachine<FSM_Character1State>
{
}
