using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 몬스터1 스턴 상태
public class FSM_Soldier_Stun : VMyState<FSM_SoldierState>
{
    public override FSM_SoldierState StateEnum => FSM_SoldierState.FSM_Soldier_Stun;
    private Soldier _soldier;
    private Coroutine _coroutine;
    
    protected override void Awake()
    {
        base.Awake();

        _soldier = GetComponent<Soldier>();
    }

    protected override void EnterState()
    {
        if (_soldier.EnemyType == EnemyType.Soldier)
        {
            _soldier._animator.CrossFade(_soldier.StunHash, 0.0f);
        }
        _coroutine = StartCoroutine(Stun());

        if (_soldier.stunEffectGameObject == null) // 스턴이 중첩되는 경우 값이 오염되는 것을 방지
        {
            if (_soldier.BossType == BossType.Regular)
            {
                _soldier.stunEffectGameObject = EffectManager.Instance.PlayEffectPositionUp(10, gameObject.transform.position, 4); // 스턴 이펙트 활성화
            }
            else if (_soldier.BossType == BossType.Boss)
            {
                Vector3 boosTypePosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
                _soldier.stunEffectGameObject = EffectManager.Instance.PlayEffectPositionUp(11, boosTypePosition, 4);
            }
        }
    }

    protected override void ExcuteState()
    {
        
    }

    protected override void ExitState()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    IEnumerator Stun()
    {
        yield return new WaitForSeconds(_soldier.StunTime);
        _coroutine = null;
        _soldier.OnStunFInish();
        
        // 스턴이 끝날 때 파티클을 비활성화
        if (_soldier.stunEffectGameObject != null)
        {
            EffectManager.Instance.DeactivateEffect(_soldier.stunEffectGameObject);
            _soldier.stunEffectGameObject = null; // 변수 초기화
        }
    }
    
    public override void OnNotify<T1, T2>(T1 t, T2 data)
    {
        if (t is SoldierNotify soldierNotify && soldierNotify == SoldierNotify.Stun && data is StunData_Soldier stunData)
        {
            // stunTime 값을 Soldier 객체에 저장
            _soldier.StunTime = stunData.StunTime;

            // 상태를 Stun 상태로 전환
            OwnerStateMachine.ChangeState(FSM_SoldierState.FSM_Soldier_Stun);
        }
    }
}
