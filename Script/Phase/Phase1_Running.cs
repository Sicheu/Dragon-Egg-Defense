using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

// 몬스터 스폰을 관리
public class Phase1_Running  : VMyState<PhaseState>
{
    public override PhaseState StateEnum => PhaseState.Phase1_Running;
    
    public int SpawnMonsterCount = 10;
    public float SpawnInterval = 2.0f;
    
    private int index;

    protected override void EnterState()
    {
        StartCoroutine(SpawnMonsters());
    }

    IEnumerator SpawnMonsters()
    {
        for (int j = 0; j < MyPlayerController.Instance.Monsters.Count; j++)
        {
            if ((j % 10) + 1 == 10) // 10번째 배열에서 보스 몹 소환하는 메커니즘
            {
                MyPlayerController.Instance.GetNewMonster(j,transform.position, Quaternion.LookRotation(Vector3.right));
                yield return new WaitForSeconds(UiManager.Instance.countdownTime); // UI 에서 설정된 웨이브 쿨타임만큼 대기
                continue;
            }
            else // 그 외 배열에서 10명의 일반 적 유닛을 소환하는 메커니즘
            {
                for (int i = 0; i < SpawnMonsterCount; ++i)
                {
                    MyPlayerController.Instance.GetNewMonster(j,transform.position, Quaternion.LookRotation(Vector3.right));
                    yield return new WaitForSeconds(SpawnInterval); // 2초 간격으로 생성
                }
            }
            yield return new WaitForSeconds(UiManager.Instance.countdownTime - (SpawnInterval * SpawnMonsterCount)); // UI 에서 설정된 웨이브 쿨타임 - 웨이브 소환하는데 걸린 시간
        }
        Debug.Log("모든 웨이브 종료");
    }

    protected override void ExcuteState()
    {
    }

    protected override void ExitState()
    {
        var data = GetEnumValue<FSM_Monster1State>();
    }

    public T GetEnumValue<T>() where T : Enum
    {
        throw new NotImplementedException();
    }
}