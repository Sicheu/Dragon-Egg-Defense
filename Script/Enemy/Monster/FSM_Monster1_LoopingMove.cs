using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//몬스터1 루프 이동 상태
public class FSM_Monster1_LoopingMove : VMyState<FSM_Monster1State>
{
    public override FSM_Monster1State StateEnum => FSM_Monster1State.FSM_Monster1_LoopingMove;
    private Monster1 monster1;

    protected override void Awake()
    {
        base.Awake();
        monster1 = GetComponent<Monster1>();
    }

    protected override void EnterState()
    {
    }

    protected override void ExcuteState()
    {
    }

    // protected override void ExcuteState_FixedUpdate()
    // {
    //     (int, Vector3) destinationInfo = PhaseManager.Instance.GetDestination(monster1.DestinationIndex); // 인덱스를 넣어 배열 범위 내인지를 확인 > 벗어나면 한바퀴 돌았으므로 재설정되어 0이됨
    //     monster1.DestinationIndex = destinationInfo.Item1; // 설정된 인덱스 번호 변수에 재등록
    //     if (monster1.MoveToDestination(destinationInfo.Item2)) // 반환된 인덱스 번호에 따라 목적지 위치 정보 Move~ 메서드에 등록, 목적지에 도달 했다면 = 트루라면
    //     {
    //         monster1.DestinationIndex++; // 인덱스 번호 1 증가하여 다음 목적지 설정할 수 있도록 함
    //     }
    // }

    protected override void ExcuteState_LateUpdate()
    {
    }

    protected override void ExitState()
    {
    }

    // OnNotify 사용 예시
    public override void OnNotify<T1, T2>(T1 t, T2 data)
    {
        switch (t)
        {
            case Monster1Notify.Stun:
                OwnerStateMachine.ChangeState(FSM_Monster1State.FSM_Monster1_Stun);
                break;
        }
    }
}
