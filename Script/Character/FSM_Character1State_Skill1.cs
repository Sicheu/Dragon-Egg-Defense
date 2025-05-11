using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 캐릭터1 스킬1 상태
public class FSM_Character1State_Skill1 :  VMyState<FSM_Character1State>
{
    public override FSM_Character1State StateEnum => FSM_Character1State.FSM_Character1State_Skill1;

    private Animator _animator;
    private Character1 _character1;
    private Rigidbody _rb;

    private Coroutine _coroutine;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();
        _character1 = GetComponent<Character1>();
        _rb = GetComponent<Rigidbody>();
    }

    protected override void EnterState()
    {   
        _character1.activeSkillInstance.StartCooltime(); // 현재 활성화된 스킬 인스턴스의 쿨타임 코루틴 시작
        
        // 애니메이션 초기화
        _animator.Rebind();
        _animator.Update(0.0f);
        
        _animator.CrossFade(_character1.activeSkillInstance.info.AnimationName_Hash, 0.0f); // 스킬 인포에 할당된 애니메이션 재생
        _coroutine = StartCoroutine(AnimationFinishCheck()); // 애니메이션이 종료되었는지 확인하는 코루틴 시작
    }

    protected override void ExcuteState()
    {
        
    }

    protected override void ExitState()
    {
        StopCoroutine(_coroutine); // 상태가 종료될 때, 실행중인 코루틴을 종료한다
    }

    protected override void ExcuteState_FixedUpdate()
    {
        base.ExcuteState_FixedUpdate(); // 상위 클래스의 FixedUpdate 호출

        if (_character1.activeSkillInstance.target) // 현재 활성화된 스킬 인스턴스에 타겟이 있다면
        {
            // 그 타겟을 바라본다
            _rb.MoveRotation(Quaternion.LookRotation((_character1.activeSkillInstance.target.transform.position - transform.position).normalized));
        }
    }

    IEnumerator AnimationFinishCheck() // 애니메이션이 끝났는지 확인하는 코루틴
    {
        yield return null;
        while (true)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 애니메이션의 현재 상태 정보를 가져온다
            if (stateInfo.shortNameHash != _character1.activeSkillInstance.info.AnimationName_Hash)
            {
                break; // 현재 애니메이션 상태가 스킬 애니메이션이 아닌 경우 루프를 종료
            }

            if (stateInfo.normalizedTime >= 1.0f)
                break; // 애니메이션이 끝난 경우 루프를 종료

            yield return null;
        }
        
        _character1.Fsm.ChangeState(FSM_Character1State.FSM_Character1State_Idle); // 애니메이션이 끝난 후 Idle 로 상태 전환
    }
}