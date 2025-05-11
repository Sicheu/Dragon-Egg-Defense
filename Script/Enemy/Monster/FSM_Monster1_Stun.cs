using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 몬스터1 스턴 상태
public class FSM_Monster1_Stun : VMyState<FSM_Monster1State>
{
    public override FSM_Monster1State StateEnum { get; }
    private Action_Stun actionStunInsance;
    protected override void Awake()
    {
        base.Awake();
        
        //actionStunInsance = new Action_Stun(gameObject);
    }

    protected override void EnterState()
    {
        actionStunInsance.EnterAction();
    }

    protected override void ExcuteState()
    {
        actionStunInsance.ExitAction();
    }

    protected override void ExitState()
    {
        actionStunInsance.ExitAction();
    }
}
