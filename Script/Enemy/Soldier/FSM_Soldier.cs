using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 몬스터1 StateMachine
public enum FSM_SoldierState
{
    None, 
    FSM_Soldier_LoopingMove,
    FSM_Soldier_Stun,
    FSM_Soldier_Dead
}

public class FSM_Soldier : StateMachine<FSM_SoldierState>
{
}
