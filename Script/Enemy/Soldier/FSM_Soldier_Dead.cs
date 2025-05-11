using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Soldier_Dead : VMyState<FSM_SoldierState>
{
    public override FSM_SoldierState StateEnum => FSM_SoldierState.FSM_Soldier_Dead;
    
    private Soldier _soldier;
    private Coroutine _coroutine;

    protected override void Awake()
    {
        base.Awake();
        _soldier = GetComponent<Soldier>();
    }

    protected override void EnterState()
    {
        // 유닛이 죽을 때 coin 을 증가시키는 필드
        if (_soldier.BossType == BossType.Regular)
        {
            if (_soldier.EnemyType == EnemyType.Soldier)
            {
                UiManager.Instance.coin += 15;
            }
            else if (_soldier.EnemyType == EnemyType.Tank)
            {
                UiManager.Instance.coin += 20;
            }
            else if (_soldier.EnemyType == EnemyType.Plane)
            {
                UiManager.Instance.coin += 30;
            }
        }
        else if (_soldier.BossType == BossType.Boss)
        {
            if (_soldier.EnemyType == EnemyType.Soldier)
            {
                UiManager.Instance.coin += 500;
                UiManager.Instance.jewel += 1;
            }
            else if (_soldier.EnemyType == EnemyType.Plane)
            {
                UiManager.Instance.coin += 1000;
                UiManager.Instance.jewel += 1;
            }
        }
        
        _soldier.NotifyRedDragonBabyOnDeath(); // 타겟 해제 및 인스턴스 제거
        
        // 애니메이션 or 이펙트 필드
        if (_soldier.EnemyType == EnemyType.Soldier)
        {
            // 애니메이션 초기화
            _soldier._animator.Rebind();
            _soldier._animator.Update(0.0f);
            
            _soldier._animator.CrossFade(_soldier.DeadHash, 0.0f); // 스킬 인포에 할당된 애니메이션 재생
            _coroutine = StartCoroutine(AnimationFinishCheck()); // 애니메이션이 종료되었는지 확인하는 코루틴 시작
            
            SoundManager.Instance.PlaySound_RandomIndex(new int[] { 7, 8 });
        }
        else if (_soldier.EnemyType == EnemyType.Tank)
        {
            EffectManager.Instance.PlayEffect(9, transform.position);
            SoundManager.Instance.PlaySound(9);
            Destroy(gameObject);
        }
        else if (_soldier.EnemyType == EnemyType.Plane)
        {
            EffectManager.Instance.PlayEffect(9, transform.position);
            SoundManager.Instance.PlaySound_RandomIndex(new int[] { 10, 11 });
            Destroy(gameObject);
        }

        // Dead 상태 진입 시 스턴 이펙트 끄기 (스턴상태에서 데드 진입시 스턴 이펙트 사라지지 않는 현상 방지)
        if (_soldier.stunEffectGameObject != null)
        {
            EffectManager.Instance.DeactivateEffect(_soldier.stunEffectGameObject);
            _soldier.stunEffectGameObject = null;
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
    
    IEnumerator AnimationFinishCheck() // 애니메이션이 끝났는지 확인하는 코루틴
    {
        yield return null;
        while (true)
        {
            var stateInfo = _soldier._animator.GetCurrentAnimatorStateInfo(0); // 애니메이션의 현재 상태 정보를 가져온다
            if (stateInfo.shortNameHash != _soldier.DeadHash)
            {
                break; // 현재 애니메이션 상태가 스킬 애니메이션이 아닌 경우 루프를 종료
            }

            if (stateInfo.normalizedTime >= 1.0f)
                
                break; // 애니메이션이 끝난 경우 루프를 종료
            

            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // Dead 상태에서 Stun 불려올 때 오류 방지
    public override void OnNotify<T1, T2>(T1 t, T2 data)
    {
        switch (t)
        {
            case SoldierNotify.Stun:
                break;
        }
    }
}
