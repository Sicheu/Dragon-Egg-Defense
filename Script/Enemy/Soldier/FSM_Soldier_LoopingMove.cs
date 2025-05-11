using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//몬스터1 루프 이동 상태
public class FSM_Soldier_LoopingMove : VMyState<FSM_SoldierState>
{
    public override FSM_SoldierState StateEnum => FSM_SoldierState.FSM_Soldier_LoopingMove;
    private Soldier _soldier;

    protected override void Awake()
    {
        base.Awake();
        _soldier = GetComponent<Soldier>();
    }

    protected override void EnterState()
    {
        if (_soldier.EnemyType == EnemyType.Soldier)
        {
            _soldier._animator.CrossFade(_soldier.WalkHash, 0.0f);
        }
    }

    protected override void ExcuteState()
    {
    }

    protected override void ExcuteState_FixedUpdate()
    {
        (int, Vector3) destinationInfo = PhaseManager.Instance.GetDestination(_soldier.DestinationIndex, _soldier); // 인덱스를 넣어 배열 범위 내인지를 확인 > 벗어나면 한바퀴 돌았으므로 재설정되어 0이됨
        _soldier.DestinationIndex = destinationInfo.Item1; // 설정된 인덱스 번호 변수에 재등록
        if (_soldier.MoveToDestination(destinationInfo.Item2)) // 반환된 인덱스 번호에 따라 목적지 위치 정보 Move~ 메서드에 등록, 목적지에 도달 했다면 = 트루라면
        {
            _soldier.DestinationIndex++; // 인덱스 번호 1 증가하여 다음 목적지 설정할 수 있도록 함
        }
    }

    protected override void ExcuteState_LateUpdate()
    {
    }

    protected override void ExitState()
    {
    }

    public override void OnNotify<T1, T2>(T1 t, T2 data)
    {
        if (t is SoldierNotify soldierNotify && soldierNotify == SoldierNotify.Stun && data is StunData_Soldier stunData)
        {
            // stunTime 값을 Soldier 객체에 저장
            _soldier.StunTime = stunData.StunTime;

            // 상태를 Stun 상태로 전환
            OwnerStateMachine.ChangeState(FSM_SoldierState.FSM_Soldier_Stun);
        }
    }
    
    // OnNotify 사용 예시
    // public override void OnNotify<T1, T2>(T1 t, T2 data)
    // {
    //     switch (t)
    //     {
    //         case SoldierNotify.Stun:
    //           
    //             OwnerStateMachine.ChangeState(FSM_SoldierState.FSM_Soldier_Stun);
    //             break;
    //     }
    // }
}
