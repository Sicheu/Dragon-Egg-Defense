using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 캐릭터1 이동 상태 : 그리드 단위로 서로 위치를 바꾸면서 움직임
public class FSM_Character1State_MoveToDestination :  VMyState<FSM_Character1State>
{
    public override FSM_Character1State StateEnum => FSM_Character1State.FSM_Character1State_MoveToDestination;

    private Character1 _character1;
    private Rigidbody _rb;
    
    protected override void Awake()
    {
        base.Awake();

        _character1 = GetComponent<Character1>();
        _rb = GetComponent<Rigidbody>();
    }

    protected override void EnterState()
    {
        _character1._animator.CrossFade(Character1.runHash, 0.0f);
    }

    protected override void ExcuteState()
    {
        Vector3 nextPostion = Vector3.MoveTowards(transform.position, _character1.Destination, Time.deltaTime * _character1.Speed); // MoveTowards 를 사용하여 이동할 목적지 좌표를 계산하고, 이동할 속도를 설정하여 변수에 저장
        _rb.MovePosition(nextPostion); // 설정된 목적지로 캐릭터 이동
        _rb.MoveRotation(Quaternion.LookRotation(nextPostion - transform.position).normalized); // 캐릭터 시선은 나아가는 방향으로 설정

        if (Vector3.Distance(transform.position, _character1.Destination) <= 1.0f)
        {
            OwnerStateMachine.ChangeState(FSM_Character1State.FSM_Character1State_Idle); // 목적지에 도달했다면 Idle 로 상태 전환
        }
    }

    protected override void ExitState()
    {
    }
}