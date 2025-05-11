using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// 이거 왜 여기있던건지 모르겠음
// public enum PhaseState // 페이즈 전반에 거루어져 다뤄질 이넘값
// {
//     NoneMyState,
//     Phase1_Ready,
//     Phase1_Running
// }
// 페이즈의 값, 핵심 기능 함수 등을 들고 있는 스크립트
public class PhaseManager : SceneSingleton<PhaseManager>
{
    private PhaseStateMachine _stateMachine; // 페이즈 상태 머신 참조
    public List<Transform> _destinations = new(); // 목적지들을 인스펙터에서 참조하여 저장
    
    protected override void Awake()
    {
        base.Awake();
        _stateMachine = GetComponent<PhaseStateMachine>();
    }

    public (int, Vector3) GetDestination(int index, Soldier soldier) // 몬스터가 설정된 Destination 내에서 지속적으로 돌게 만들기 위한 인덱스 재지정 튜플 메서드
    {
        int resultIndex = index;
        if (resultIndex >= _destinations.Count) // 인덱스가 리스트 범위를 벗어나면 
        {
            Soldier soldierComponent = soldier.GetComponent<Soldier>();
            soldierComponent.LoopingMoveComplete();

            resultIndex = 0; // 오류 로그 방지. 기본값을 반환. 의미는 없음
        }

        return (resultIndex, _destinations[resultIndex].position); // 설정된 인덱스와 인덱스에 해당하는 위치를 튜플로 반환
    }
}