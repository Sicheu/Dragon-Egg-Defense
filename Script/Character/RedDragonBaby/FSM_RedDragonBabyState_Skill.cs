using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_RedDragonBabyState_Skill : VMyState<FSM_RedDragonBabyState>
{
    public override FSM_RedDragonBabyState StateEnum => FSM_RedDragonBabyState.FSM_RedDragonBabyState_Skill;
    
    private Character_RedDragonBaby _cr;
    private Coroutine _coroutine;

    protected override void Awake()
    {
        base.Awake();

        _cr = GetComponent<Character_RedDragonBaby>();
    }

    protected override void EnterState()
    {   
        // 애니메이션 초기화
        _cr._animator.Rebind();
        _cr._animator.Update(0.0f);
        
        _cr._animator.CrossFade(_cr.ActiveSkillInstance.info.AnimationName_Hash, 0.0f); // 스킬 인포에 할당된 애니메이션 재싱
        _coroutine = StartCoroutine(AnimationFinishCheck()); // 애니메이션이 종료되었는지 확인하는 코루틴 시작

        if (_cr.CharacterType == CharacterType.RedDragonBaby)
        {
            EffectManager.Instance.PlayEffectShot(0, transform.position, _cr.ActiveSkillInstance.target.transform);
            SoundManager.Instance.PlaySound(0);
        }
        else if (_cr.CharacterType == CharacterType.RedDragonAdolescent)
        {
            EffectManager.Instance.PlayEffectShot(3, transform.position, _cr.ActiveSkillInstance.target.transform);
            SoundManager.Instance.PlaySound(5);
        }
        else if (_cr.CharacterType == CharacterType.RedDragonAdult)
        {
            EffectManager.Instance.PlayEffectRotation(6, transform.position, _cr.ActiveSkillInstance.target.transform);
            SoundManager.Instance.PlaySound(6);
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

        if (_cr.ActiveSkillInstance == null || _cr.ActiveSkillInstance.target == null) // null 체크 추가. target이 Dead애니메이션중에 이동으로 가까워진 객체에서 이쪽 널 오류 발생했기 때문
        {
            _cr.Fsm.ChangeState(FSM_RedDragonBabyState.FSM_RedDragonBabyState_Idle);
            return;
        }
        
        if (_cr.ActiveSkillInstance.target) // 현재 활성화된 스킬 인스턴스에 타겟이 있다면
        {
            // 그 타겟을 바라본다
            _cr._rb.MoveRotation(Quaternion.LookRotation((_cr.ActiveSkillInstance.target.transform.position - transform.position).normalized));
        }
        else
        {
            _cr.Fsm.ChangeState(FSM_RedDragonBabyState.FSM_RedDragonBabyState_Idle);
        }
    }

    IEnumerator AnimationFinishCheck() // 애니메이션이 끝났는지 확인하는 코루틴
    {
        yield return null;
        while (true)
        {
            var stateInfo = _cr._animator.GetCurrentAnimatorStateInfo(0); // 애니메이션의 현재 상태 정보를 가져온다
            if (stateInfo.shortNameHash != _cr.ActiveSkillInstance.info.AnimationName_Hash)
            {
                break; // 현재 애니메이션 상태가 스킬 애니메이션이 아닌 경우 루프를 종료
            }

            if (stateInfo.normalizedTime >= 1.0f)
                break; // 애니메이션이 끝난 경우 루프를 종료
            
            if (_cr.ActiveSkillInstance == null || _cr.ActiveSkillInstance.target == null) // null 체크
            {
                _cr.Fsm.ChangeState(FSM_RedDragonBabyState.FSM_RedDragonBabyState_Idle);
                yield break;
            }
            
            yield return null;
        }
        
        if (_cr.ActiveSkillInstance != null && _cr.ActiveSkillInstance.target != null) // null 체크
        {
            _cr.GetDamaged();
            _cr.ActiveSkillInstance.StartCooltime(); // 현재 활성화된 스킬 인스턴스의 쿨타임 코루틴 시작, 해당 위치에 있어야 애니메이션이 끝나고 쿨타임이 흐름
        }
        _cr.Fsm.ChangeState(FSM_RedDragonBabyState.FSM_RedDragonBabyState_Idle); // 애니메이션이 끝난 후 Idle 로 상태 전환
    }
}
