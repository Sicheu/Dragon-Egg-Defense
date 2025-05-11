using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_DarkDragonBabyState_Skill : VMyState<FSM_DarkDragonBabyState>
{
    public override FSM_DarkDragonBabyState StateEnum => FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Skill;

    private Character_DarkDragonBaby _cd;
    private Coroutine _coroutine;

    protected override void Awake()
    {
        base.Awake();

        _cd = GetComponent<Character_DarkDragonBaby>();
    }

    protected override void EnterState()
    {   
        // 애니메이션 초기화
        _cd._animator.Rebind();
        _cd._animator.Update(0.0f);
        
        _cd._animator.CrossFade(_cd.ActiveSkillInstance.info.AnimationName_Hash, 0.0f); // 스킬 인포에 할당된 애니메이션 재싱
        _coroutine = StartCoroutine(AnimationFinishCheck()); // 애니메이션이 종료되었는지 확인하는 코루틴 시작

        _cd.SkillSoundPlay = false; // 스킬 상태 진입시 사운드 재생 가능 상태로 설정

        for (int i = 0; i < _cd.ActiveSkillInstance.targets.Length; i++)
        {
            if (_cd.CharacterType == CharacterType.DarkDragonBaby)
            {
                EffectManager.Instance.PlayEffectShot(2, transform.position, _cd.ActiveSkillInstance.targets[i].transform);
                if (!_cd.SkillSoundPlay)
                {
                    SoundManager.Instance.PlaySound(0);
                    _cd.SkillSoundPlay = true; // 스킬이 첫 타겟에서 플레이 되었음을 알려, 중복으로 재생되는 것을 방지
                }
            }
            else if (_cd.CharacterType == CharacterType.DarkDragonAdolescent)
            {
                EffectManager.Instance.PlayEffectShot(5, transform.position, _cd.ActiveSkillInstance.targets[i].transform);
                if (!_cd.SkillSoundPlay)
                {
                    SoundManager.Instance.PlaySound(1);
                    _cd.SkillSoundPlay = true;
                }
            }
            else if (_cd.CharacterType == CharacterType.DarkDragonAdult)
            {
                EffectManager.Instance.PlayEffectFollow(8, _cd.ActiveSkillInstance.targets[i].transform, _cd.ActiveSkillInstance.targets[i]);
                if (!_cd.SkillSoundPlay)
                {
                    SoundManager.Instance.PlaySound(2);
                    _cd.SkillSoundPlay = true;
                }
            }
            else
            {
                Debug.Log("캐릭터 타입이 지정되지 않음");
            }
        }
    }

    protected override void ExcuteState()
    {
    }

    protected override void ExitState()
    {
        StopCoroutine(_coroutine); // 상태가 종료될 때, 실행중인 코루틴을 종료한다
        _cd.SkillSoundPlay = false; // 다음 스킬 진입시 사운드 재생을 위해
    }

    protected override void ExcuteState_FixedUpdate()
    {
        base.ExcuteState_FixedUpdate(); // 상위 클래스의 FixedUpdate 호출

        if (_cd.ActiveSkillInstance == null || _cd.ActiveSkillInstance.targets == null || _cd.ActiveSkillInstance.targets.Length == 0) // null 체크 추가. target이 Dead애니메이션중에 이동으로 가까워진 객체에서 이쪽 널 오류 발생했기 때문
        {
            _cd.Fsm.ChangeState(FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Idle);
            return;
        }
        
        if (_cd.ActiveSkillInstance.targets[0]) // 현재 활성화된 스킬 인스턴스에 타겟이 있다면
        {
            // 그 타겟을 바라본다
            _cd._rb.MoveRotation(Quaternion.LookRotation((_cd.ActiveSkillInstance.targets[0].transform.position - transform.position).normalized));
        }
        else
        {
            _cd.Fsm.ChangeState(FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Idle);
        }
    }

    IEnumerator AnimationFinishCheck() // 애니메이션이 끝났는지 확인하는 코루틴
    {
        yield return null;
        while (true)
        {
            var stateInfo = _cd._animator.GetCurrentAnimatorStateInfo(0); // 애니메이션의 현재 상태 정보를 가져온다
            if (stateInfo.shortNameHash != _cd.ActiveSkillInstance.info.AnimationName_Hash)
            {
                break; // 현재 애니메이션 상태가 스킬 애니메이션이 아닌 경우 루프를 종료
            }

            if (stateInfo.normalizedTime >= 1.0f)
                break; // 애니메이션이 끝난 경우 루프를 종료
            
            if (_cd.ActiveSkillInstance == null || _cd.ActiveSkillInstance.targets == null) // null 체크
            {
                _cd.Fsm.ChangeState(FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Idle);
                yield break;
            }
            
            yield return null;
        }
        
        if (_cd.ActiveSkillInstance != null && _cd.ActiveSkillInstance.targets != null) // null 체크
        {
            _cd.GetDamaged();
            _cd.ActiveSkillInstance.StartCooltime(); // 현재 활성화된 스킬 인스턴스의 쿨타임 코루틴 시작, 해당 위치에 있어야 애니메이션이 끝나고 쿨타임이 흐름
        }
        _cd.Fsm.ChangeState(FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Idle); // 애니메이션이 끝난 후 Idle 로 상태 전환
    }
}
