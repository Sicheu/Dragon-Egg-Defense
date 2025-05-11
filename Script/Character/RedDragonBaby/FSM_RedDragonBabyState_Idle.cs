using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_RedDragonBabyState_Idle : VMyState<FSM_RedDragonBabyState>
{
    public override FSM_RedDragonBabyState StateEnum => FSM_RedDragonBabyState.FSM_RedDragonBabyState_Idle;
    private Character_RedDragonBaby _cr;

    protected override void Awake()
    {
        base.Awake();

        _cr = GetComponent<Character_RedDragonBaby>();
    }

    protected override void EnterState()
    {
        //_cr._readyToMove = null; // 코루틴 오버플로우 방지를 위한 널처리
        _cr._animator.CrossFade(Character_RedDragonBaby.IdleHash, 0.0f);
    }

    // protected override void ExcuteState()
    // {
    //     foreach (var SkillInfo in _cr.SkillInstances)
    //     {
    //         if (SkillInfo.IsCooltiming()) // 쿨타임 중인 스킬 건너 뜀
    //             continue;
    //         
    //         foreach (var instanceMonster in MyPlayerController.Instance.GetMonsterList()) // 생성된 모든 몬스터를 가져와 검사
    //         {
    //             if (instanceMonster != null && SkillInfo.info.AttackDistance >=
    //                 (instanceMonster.transform.position - _cr.transform.position).magnitude) // 몬스터와 캐릭터의 거리가 AttackDistance 안이라면 !! null 체크를 안한다면 타겟이 죽어서 Destroy 된 이후에도 해당 객체를 참조하려고 하여 오류가 발생한다.
    //             {
    //                 SkillInfo.target = instanceMonster; // 스킬 인스턴스의 타겟을 범위로 들어온 몬스터로 설정하고
    //                 _cr.StartSkillInstance(SkillInfo); // 특정 스킬을 인스턴스로 설정하고 상태를 스킬로 변하게 하는 메서드 호출
    //                 return;
    //             }
    //         }
    //     }
    // }
    
    protected override void ExcuteState()
    {
        foreach (var SkillInfo in _cr.SkillInstances)
        {
            if (SkillInfo.IsCooltiming()) // 쿨타임 중인 스킬 건너 뜀
                continue;
    
            
            // 우선순위 지정 필드
            GameObject leadingTarget = null;
            int furthestIndex = -1;
            float closestDistanceToNextTarget = float.MaxValue;
    
            foreach (var instanceMonster in MyPlayerController.Instance.GetMonsterList()) // 생성된 모든 몬스터를 가져와 검사
            {
                if (instanceMonster == null)
                    continue;
    
                Soldier soldier = instanceMonster.GetComponent<Soldier>();
                if (soldier == null || soldier.isDead)
                    continue;
    
                float distanceToDragon = (instanceMonster.transform.position - _cr.transform.position).magnitude; // 사정거리 안에 들어왔는지 거리 확인
    
                // 몬스터가 공격 범위 내에 있는지 확인
                if (SkillInfo.info.AttackDistance >= distanceToDragon)
                {
                    // Soldier의 목적지 인덱스를 확인하여 가장 앞서나간 Soldier를 찾음
                    if (soldier.DestinationIndex > furthestIndex)
                    {
                        furthestIndex = soldier.DestinationIndex;
                        leadingTarget = instanceMonster;
    
                        // 다음 목적지까지의 거리 계산
                        Vector3 nextDestination = PhaseManager.Instance.GetDestination(soldier.DestinationIndex, soldier).Item2;
                        closestDistanceToNextTarget = Vector3.Distance(instanceMonster.transform.position, nextDestination);
                    }
                    else if (soldier.DestinationIndex == furthestIndex) // 동일한 인덱스일 경우 다음 목적지까지의 거리가 가까운 Soldier 선택
                    {
                        Vector3 nextDestination = PhaseManager.Instance.GetDestination(soldier.DestinationIndex, soldier).Item2;
                        float distanceToNextTarget = Vector3.Distance(instanceMonster.transform.position, nextDestination);
    
                        if (distanceToNextTarget < closestDistanceToNextTarget)
                        {
                            leadingTarget = instanceMonster;
                            closestDistanceToNextTarget = distanceToNextTarget;
                        }
                    }
                }
            }
    
            // 타겟 설정 및 스킬 실행
            if (leadingTarget != null)
            {
                SkillInfo.target = leadingTarget;
                _cr.StartSkillInstance(SkillInfo);
                return;
            }
        }
    }
    
    protected override void ExitState()
    {
    }
}
