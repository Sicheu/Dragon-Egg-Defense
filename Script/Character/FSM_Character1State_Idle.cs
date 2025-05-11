using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 캐릭터1 기본 상태 : 범위 안에 들어온 몬스터를 향해 스킬 사용
public class FSM_Character1State_Idle :  VMyState<FSM_Character1State>
{
    public override FSM_Character1State StateEnum => FSM_Character1State.FSM_Character1State_Idle;
    private Character1 _character1;
    
    protected override void Awake()
    {
        base.Awake();

        _character1 = GetComponent<Character1>();
    }

    protected override void EnterState()
    {
        _character1._animator.CrossFade(Character1.IdleHash, 0.0f); // 캐릭터의 애니메이션을 대기 상태로 전환
    }

    protected override void ExcuteState()
    {
        foreach (var character1SkillInfo in _character1.skillInstances)
        {
            if (character1SkillInfo.IsCooltiming()) // 쿨타임 중인 스킬 건너 뜀
                continue;
            
            foreach (var instanceMonster in MyPlayerController.Instance.GetMonsterList()) // 생성된 모든 몬스터를 가져와 검사
            {
                if (character1SkillInfo.info.AttackDistance >=
                    (instanceMonster.transform.position - _character1.transform.position).magnitude) // 몬스터와 캐릭터의 거리가 AttackDistance 안이라면
                {
                    character1SkillInfo.target = instanceMonster; // 스킬 인스턴스의 타겟을 범위로 들어온 몬스터로 설정하고
                    _character1.StartSkillInstance(character1SkillInfo); // 특정 스킬을 인스턴스로 설정하고 상태를 스킬1로 변하게 하는 메서드 호출
                    return;
                }
            }
        }
    }

    protected override void ExitState()
    {
    }
}