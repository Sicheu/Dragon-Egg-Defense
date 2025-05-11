using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_GreenDragonBabyState_Skill : VMyState<FSM_GreenDragonBabyState>
{
    public override FSM_GreenDragonBabyState StateEnum => FSM_GreenDragonBabyState.FSM_GreenDragonBabyState_Skill;

    private Character_GreenDragonBaby _cg;

    private Coroutine _coroutine;

    protected override void Awake()
    {
        base.Awake();

        _cg = GetComponent<Character_GreenDragonBaby>();
    }

    protected override void EnterState()
    {   
        // 애니메이션 초기화
        _cg._animator.Rebind();
        _cg._animator.Update(0.0f);
        
        _cg._animator.CrossFade(_cg.ActiveSkillInstance.info.AnimationName_Hash, 0.0f); // 스킬 인포에 할당된 애니메이션 재싱
        _coroutine = StartCoroutine(AnimationFinishCheck()); // 애니메이션이 종료되었는지 확인하는 코루틴 시작

        if (_cg.CharacterType == CharacterType.GreenDragonBaby)
        {
            EffectManager.Instance.PlayEffectShot(1, transform.position, _cg.ActiveSkillInstance.target.transform);
            SoundManager.Instance.PlaySound(0);
        }
        else if (_cg.CharacterType == CharacterType.GreenDragonAdolescent)
        {
            EffectManager.Instance.PlayEffectShot(4, transform.position, _cg.ActiveSkillInstance.target.transform);
            SoundManager.Instance.PlaySound(3);
        }
        else if (_cg.CharacterType == CharacterType.GreenDragonAdult)
        {
            EffectManager.Instance.PlayEffectShot(7, transform.position, _cg.ActiveSkillInstance.target.transform);
            SoundManager.Instance.PlaySound(4);
        }
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

        if (_cg.ActiveSkillInstance == null || _cg.ActiveSkillInstance.target == null) // null 체크 추가. target이 Dead애니메이션중에 이동으로 가까워진 객체에서 이쪽 널 오류 발생했기 때문
        {
            _cg.Fsm.ChangeState(FSM_GreenDragonBabyState.FSM_GreenDragonBabyState_Idle);
            return;
        }
        
        if (_cg.ActiveSkillInstance.target) // 현재 활성화된 스킬 인스턴스에 타겟이 있다면
        {
            // 그 타겟을 바라본다
            _cg._rb.MoveRotation(Quaternion.LookRotation((_cg.ActiveSkillInstance.target.transform.position - transform.position).normalized));
        }
        else
        {
            _cg.Fsm.ChangeState(FSM_GreenDragonBabyState.FSM_GreenDragonBabyState_Idle);
        }
    }

    IEnumerator AnimationFinishCheck() // 애니메이션이 끝났는지 확인하는 코루틴
    {
        yield return null;
        while (true)
        {
            var stateInfo = _cg._animator.GetCurrentAnimatorStateInfo(0); // 애니메이션의 현재 상태 정보를 가져온다
            if (stateInfo.shortNameHash != _cg.ActiveSkillInstance.info.AnimationName_Hash)
            {
                break; // 현재 애니메이션 상태가 스킬 애니메이션이 아닌 경우 루프를 종료
            }

            if (stateInfo.normalizedTime >= 1.0f)
                break; // 애니메이션이 끝난 경우 루프를 종료
            
            if (_cg.ActiveSkillInstance == null || _cg.ActiveSkillInstance.target == null) // null 체크
            {
                _cg.Fsm.ChangeState(FSM_GreenDragonBabyState.FSM_GreenDragonBabyState_Idle);
                yield break;
            }
            
            yield return null;
        }
        
        if (_cg.ActiveSkillInstance != null && _cg.ActiveSkillInstance.target != null) // null 체크
        {
            _cg.GetDamaged();
            _cg.ActiveSkillInstance.StartCooltime(); // 현재 활성화된 스킬 인스턴스의 쿨타임 코루틴 시작, 해당 위치에 있어야 애니메이션이 끝나고 쿨타임이 흐름
        }
        _cg.Fsm.ChangeState(FSM_GreenDragonBabyState.FSM_GreenDragonBabyState_Idle); // 애니메이션이 끝난 후 Idle 로 상태 전환
    }
}
