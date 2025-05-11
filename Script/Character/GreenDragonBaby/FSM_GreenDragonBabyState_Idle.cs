using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_GreenDragonBabyState_Idle : VMyState<FSM_GreenDragonBabyState>
{
    public override FSM_GreenDragonBabyState StateEnum => FSM_GreenDragonBabyState.FSM_GreenDragonBabyState_Idle;
    private Character_GreenDragonBaby _cg;

    protected override void Awake()
    {
        base.Awake();

        _cg = GetComponent<Character_GreenDragonBaby>();
    }

    protected override void EnterState()
    {
        _cg._animator.CrossFade(Character_GreenDragonBaby.IdleHash, 0.0f);
    }
    
    // protected override void ExcuteState()
    // {
    //     foreach (var SkillInfo in _cg.SkillInstances)
    //     {
    //         if (SkillInfo.IsCooltiming()) // 쿨타임 중인 스킬 건너 뜀
    //             continue;
    //
    //         GameObject bestTarget = null; // 스턴 상태가 아닌 가장 가까운 타겟
    //         GameObject fallbackTarget = null; // 모든 몬스터가 스턴일 경우, 가장 가까운 타겟
    //
    //         foreach (var instanceMonster in MyPlayerController.Instance.GetMonsterList()) // 생성된 모든 몬스터를 가져와 검사
    //         {
    //             Soldier soldier = instanceMonster.GetComponent<Soldier>();
    //
    //             if (instanceMonster != null)
    //             {
    //                 float distance = (instanceMonster.transform.position - _cg.transform.position).magnitude;
    //
    //                 // 스턴 상태가 아닌 타겟을 우선적으로 선택
    //                 if (!soldier.isStunned && SkillInfo.info.AttackDistance >= distance)
    //                 {
    //                     bestTarget = instanceMonster; // 첫 번째로 발견된 범위 내 스턴 상태가 아닌 타겟을 설정
    //                     break; // 범위 내 첫 번째 타겟을 찾으면 더 이상 검사하지 않음
    //                 }
    //
    //                 // Fallback 타겟은 첫 번째로 발견된 범위 내 몬스터로 설정 (Stun 상태 여부와 상관없이)
    //                 if (fallbackTarget == null && SkillInfo.info.AttackDistance >= distance)
    //                 {
    //                     fallbackTarget = instanceMonster;
    //                 }
    //             }
    //         }
    //
    //         // Stun 상태가 아닌 타겟이 있으면 그 타겟을 공격, 없으면 첫 번째로 발견된 범위 내 몬스터 공격
    //         GameObject finalTarget = bestTarget ?? fallbackTarget;
    //
    //         if (finalTarget != null && SkillInfo.info.AttackDistance >= (finalTarget.transform.position - _cg.transform.position).magnitude)
    //         {
    //             SkillInfo.target = finalTarget; // 타겟으로 설정
    //             _cg.StartSkillInstance(SkillInfo); // 스킬 시작
    //             return;
    //         }
    //     }
    // }
    
    protected override void ExcuteState()
    {
        foreach (var SkillInfo in _cg.SkillInstances)
        {
            if (SkillInfo.IsCooltiming()) // 쿨타임 중인 스킬 건너뜀
                continue;

            GameObject bestTarget = null; // 스턴 상태가 아닌 가장 가까운 타겟

            // 공격 범위 내에 있는 Soldier들을 정렬하여 우선순위를 설정
            List<Soldier> sortedSoldiers = new List<Soldier>();

            foreach (var instanceMonster in MyPlayerController.Instance.GetMonsterList()) // 생성된 모든 몬스터를 가져와 검사
            {
                if (instanceMonster == null)
                    continue;

                Soldier soldier = instanceMonster.GetComponent<Soldier>();
                if (soldier == null || soldier.isDead)
                    continue;

                float distanceToDragon =
                    (instanceMonster.transform.position - _cg.transform.position).magnitude; // 사정거리 안에 들어왔는지 거리 확인

                if (SkillInfo.info.AttackDistance >= distanceToDragon)
                {
                    sortedSoldiers.Add(soldier); // 조건에 맞는 Soldier를 리스트에 추가
                }
            }

            // Soldier들을 DestinationIndex와 다음 목적지까지의 거리 기준으로 정렬 !! 비교-정렬 알고리즘을 사용하는 Sort(Comparison<T>) 를 사용하여 리스트를 정렬
            sortedSoldiers.Sort((soldier1, soldier2) =>
            {
                if (soldier1.DestinationIndex == soldier2.DestinationIndex) // 목적지 인덱스가 같다면, 목적지에 더 가까운 soldier 를 앞으로
                {
                    // 둘의 목적지의 위치를 찾음
                    Vector3 nextDestination1 =
                        PhaseManager.Instance.GetDestination(soldier1.DestinationIndex, soldier1).Item2;
                    Vector3 nextDestination2 =
                        PhaseManager.Instance.GetDestination(soldier2.DestinationIndex, soldier2).Item2;

                    // 둘의 목적지의 위치와 현재 위치를 비교해서 거리를 구함
                    float distance1 = Vector3.Distance(soldier1.transform.position, nextDestination1);
                    float distance2 = Vector3.Distance(soldier2.transform.position, nextDestination2);

                    return distance1.CompareTo(distance2); // 거리가 가까운 순으로 정렬
                }

                return soldier2.DestinationIndex.CompareTo(soldier1.DestinationIndex); // DestinationIndex가 큰 순으로 정렬
            });
            
            for (int i = 0; i < sortedSoldiers.Count; i++) // 정렬된 리스트를 순서대로 돌며 bestTarget 찾기
            {
                if (!sortedSoldiers[i].isStunned)
                {
                    bestTarget = sortedSoldiers[i].gameObject;
                    break;
                }
            }

            // 타겟 설정 및 스킬 실행
            if (bestTarget != null)
            {
                SkillInfo.target = bestTarget;
                _cg.StartSkillInstance(SkillInfo);
                return;
            }
            else if (bestTarget == null && sortedSoldiers.Count > 0)
            {
                SkillInfo.target = sortedSoldiers[0].gameObject;
                _cg.StartSkillInstance(SkillInfo);
                return;
            }
        }
    }
    
    protected override void ExitState()
    {
    }
}
