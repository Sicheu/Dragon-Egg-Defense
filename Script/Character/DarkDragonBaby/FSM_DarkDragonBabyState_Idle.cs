using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_DarkDragonBabyState_Idle : VMyState<FSM_DarkDragonBabyState>
{
    public override FSM_DarkDragonBabyState StateEnum => FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Idle;
    
    private Character_DarkDragonBaby _cd;

    protected override void Awake()
    {
        base.Awake();

        _cd = GetComponent<Character_DarkDragonBaby>();
    }

    protected override void EnterState()
    {
        _cd._animator.CrossFade(Character_DarkDragonBaby.IdleHash, 0.0f);
    }

    // protected override void ExcuteState()
    // {
    //     foreach (var SkillInfo in _cd.SkillInstances)
    //     {
    //         if (SkillInfo.IsCooltiming()) // 쿨타임 중인 스킬 건너 뜀
    //             continue;
    //
    //         List<GameObject> targets = new List<GameObject>(); // 멀티 타겟을 저장해둘 리스트
    //         
    //         foreach (var instanceMonster in MyPlayerController.Instance.GetMonsterList()) // 생성된 모든 몬스터를 가져와 검사
    //         {
    //             if (instanceMonster != null && SkillInfo.info.AttackDistance >=
    //                 (instanceMonster.transform.position - _cd.transform.position).magnitude) // 몬스터와 캐릭터의 거리가 AttackDistance 안이라면 !! null 체크를 안한다면 타겟이 죽어서 Destroy 된 이후에도 해당 객체를 참조하려고 하여 오류가 발생한다.
    //             {
    //                 targets.Add(instanceMonster);
    //                 
    //                 if (targets.Count >= SkillInfo.info.MultipeTargetCount)
    //                 {
    //                     break;
    //                 }
    //             }
    //         }
    //
    //         if (targets.Count > 0) // 이 조건문이 없다면, targets 리스트에 배열값이 없는 상태(단, 이 상태가 null 은 아님. 때문에 카운트로 조건문 설정)이더라도 SkillState 로 넘어가기 때문에 지속적으로 AttackFireBall 애니메이션을 뱉는 버그가 발생
    //         {
    //             SkillInfo.targets = targets.ToArray();
    //             _cd.StartSkillInstance(SkillInfo);
    //         }
    //     }
    // }

    protected override void ExcuteState()
    {
        foreach (var SkillInfo in _cd.SkillInstances)
        {
            if (SkillInfo.IsCooltiming()) // 쿨타임 중인 스킬 건너뜀
                continue;

            // 목표로 설정할 타겟들 리스트
            List<GameObject> targets = new List<GameObject>();

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
                    (instanceMonster.transform.position - _cd.transform.position).magnitude; // 사정거리 안에 들어왔는지 거리 확인

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

            // 정렬된 Soldier 중 상위 MultipeTargetCount만큼 타겟 리스트에 추가
            for (int i = 0; i < SkillInfo.info.MultipeTargetCount && i < sortedSoldiers.Count; i++) // 멀티 타겟의 카운트나, 범위 내의 몬스터의 수가 허락하는 수만큼 타겟으로 지정
            {
                targets.Add(sortedSoldiers[i].gameObject);
            }

            // 타겟 설정 및 스킬 실행
            if (targets.Count > 0)
            {
                SkillInfo.targets = targets.ToArray(); // 다중 타겟 설정
                _cd.StartSkillInstance(SkillInfo, targets);
                return;
            }
        }
    }

    protected override void ExitState()
    {
    }
}
