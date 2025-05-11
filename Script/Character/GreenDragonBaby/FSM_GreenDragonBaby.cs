using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 그린드래곤베이비 상태머신
public enum FSM_GreenDragonBabyState
{
    FSM_GreenDragonBabyState_Idle,
    FSM_GreenDragonBabyState_Skill,
    FSM_GreenDragonBabyState_MoveToPoint
}

public class FSM_GreenDragonBaby : StateMachine<FSM_GreenDragonBabyState>
{
}
