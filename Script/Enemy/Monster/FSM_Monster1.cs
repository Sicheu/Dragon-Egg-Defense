using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 몬스터1 StateMachine
public enum FSM_Monster1State
{
    None, 
    FSM_Monster1_LoopingMove,
    FSM_Monster1_Stun
}

public class FSM_Monster1 : StateMachine<FSM_Monster1State>
{
}
