using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// 페이즈매니저에서 몬스터를 스폰하기 전, 3초동안 대기하게 하는 스크립트
public class Phase1_Ready : VMyState<PhaseState>
{
    public override PhaseState StateEnum => PhaseState.Phase1_Ready;

    protected override void EnterState()
    {
        StartCoroutine(GoToNextState());
    }

    IEnumerator GoToNextState()
    {
        //yield return new WaitForSeconds(UiManager.Instance.countdownTime);
        yield return new WaitForSeconds(0);
        OwnerStateMachine.ChangeState(PhaseState.Phase1_Running);
    }

    protected  override void ExcuteState()
    {
    }

    protected  override void ExitState()
    {
    }
}