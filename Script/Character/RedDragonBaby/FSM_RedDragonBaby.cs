using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 레드드래곤베이비 상태머신
public enum FSM_RedDragonBabyState
{
    None,
    FSM_RedDragonBabyState_Idle,
    FSM_RedDragonBabyState_Skill,
    FSM_RedDragonBabyState_MoveToPoint

}

public class FSM_RedDragonBaby : StateMachine<FSM_RedDragonBabyState>
{
}
