using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_RedDragonBaby : CharacterBase<FSM_RedDragonBaby>, ISelectable
{
    public CharacterType characterType; // 인스펙터에서 지정한 값을 받아오는 부분 
    public CharacterType CharacterType => characterType; // 이넘값을 지정하는 부분
    
    public List<SkillInfo> SkillInfos;

    public List<SkillInstance> SkillInstances;
    public SkillInstance ActiveSkillInstance;

    public float Speed = 30.0f;
    
    public LayerMask floorLayerMask;

    public bool Selecting = false; // 선택 중임을 나타내는 변수
    
    public static readonly int IdleHash = Animator.StringToHash("Idle");
    public static readonly int MoveHash = Animator.StringToHash("Fly Forward");

    protected override void Awake()
    {
        base.Awake();

        foreach (var skillInfo in SkillInfos)
        {
            SkillInstance inst = gameObject.AddComponent<SkillInstance>();
            inst.info = skillInfo;
            SkillInstances.Add(inst);
        }

        _animator = GetComponent<Animator>();
    }

    public void StartSkillInstance(SkillInstance instance)
    {
        ActiveSkillInstance = instance;
        Fsm.ChangeState(FSM_RedDragonBabyState.FSM_RedDragonBabyState_Skill);
    }
    
    public void GetDamaged()
    {
        if (ActiveSkillInstance.target != null) // target이 null인지 확인 = 타겟이 Dead 했을 때 오류 발생
        {
            Soldier targetSoldier = ActiveSkillInstance.target.GetComponent<Soldier>();
            if (targetSoldier != null)
            {
                targetSoldier.Dameged(ActiveSkillInstance.info.Damage);
            }
        }
        else
        {
            Debug.LogWarning("ActiveSkillInstance.target is null");
        }
    }

    public void ClearTarget()
    {
        ActiveSkillInstance = null;
    }
    
    public void OnSelected() // 캐릭터가 선택되었을때 불려오는 메서드, MoveToPoint 상태로 이동할 준비를 하는 코루틴을 작동시킨다.
    {
        if (!Selecting) // 선택 매니저에서 드래그 중일때 OnSelected 를 여러번 불러옴. 이에 리스트에 다중으로 등록되는 것을 방지
        {
            MyPlayerController.Instance.SelectingOnController(this, gameObject); // 해당 유닛이 선택된 상태임을 컨트롤러에 알림
        }
        
        Selecting = true;
        
        if (_readyToMove != null)
        {
            StopCoroutine(_readyToMove);
        }
        
        _readyToMove = StartCoroutine(ReadyToMove());
    }

    public void DeSelected() // 캐릭터가 선택 해제되었을 때 불려오는 메서드. 코루틴을 중지시킨다.
    {
        Selecting = false;
        
        if (_readyToMove != null)
        {
            StopCoroutine(_readyToMove);
            _readyToMove = null;
        }
        
        MyPlayerController.Instance.DeSelectingOnController(this, gameObject); // 유닛 선택이 해제 되었음을 컨트롤러에 알림
    }

    public IEnumerator ReadyToMove() // 선택된 상태에서 Idle 상태와 Skill 상태를 유지하면서 언제든지 MoveToPoint 로 이동할 수 있도록 하기 위해 만든 코루틴
    {
        while (true)
        {
            if (Input.GetMouseButton(1))
            {
                Fsm.ChangeState(FSM_RedDragonBabyState.FSM_RedDragonBabyState_MoveToPoint);
                yield break; // 코루틴 간섭 방지를 위해 상태가 변경될 때 일단 코루틴을 중지시키고, 상태가 종료될 때 다시 작동시킨다.
            }

            yield return null;
        }
    }
}
