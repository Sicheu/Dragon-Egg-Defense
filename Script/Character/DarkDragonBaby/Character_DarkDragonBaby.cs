using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_DarkDragonBaby : CharacterBase<FSM_DarkDragonBaby>, ISelectable
{
    //public CharacterType CharacterType => CharacterType.DarkDragonBaby; // 기존 코드는 타입을 스크립트 당 하나로밖에 고정하지 못했으므로 유연성이 떨어졌음
    // 인스펙터에서 CharacterType 을 설정할 수 있도록 스크립트에서 필드로 선언
    public CharacterType characterType; // 인스펙터에서 지정한 값을 받아오는 부분 
    public CharacterType CharacterType => characterType; // 이넘값을 지정하는 부분
    
    public List<SkillInfo> SkillInfos;

    public List<SkillInstance> SkillInstances;
    public SkillInstance ActiveSkillInstance;

    public float Speed = 30.0f;
    
    public LayerMask floorLayerMask;

    public bool Selecting = false; // 이 유닛이 SelectManager 에 의해 선택 중인 상태인지 나타내는 변수
    
    public List<GameObject> targets = new List<GameObject>(); // Idle 상태에서 지정한 타겟들 받아올 리스트
    
    public static readonly int IdleHash = Animator.StringToHash("Idle");
    public static readonly int MoveHash = Animator.StringToHash("Fly Forward");

    public bool SkillSoundPlay = false; // 스킬 사운드가 재생됬는지 확인

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

    public void StartSkillInstance(SkillInstance instance, List<GameObject> targets)
    {
        this.targets = targets;
        ActiveSkillInstance = instance;
        Fsm.ChangeState(FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Skill);
    }
    
    public void GetDamaged()
    {
        if (ActiveSkillInstance.targets != null) // target이 null인지 확인 = 타겟이 Dead 했을 때 오류 발생
        {
            for (int i = 0; i < ActiveSkillInstance.targets.Length; i++)
            {
                Soldier targetSoldier = ActiveSkillInstance.targets[i].GetComponent<Soldier>();
                if (targetSoldier != null)
                {
                    targetSoldier.Dameged(ActiveSkillInstance.info.Damage);
                }
            }
        }
        else
        {
            Debug.LogWarning("ActiveSkillInstance.target is null");
        }
    }

    public void ClearTarget()
    {
        if (ActiveSkillInstance != null)
        {
            ActiveSkillInstance.targets = null;
        }
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
            _readyToMove = null;
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
                Fsm.ChangeState(FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_MoveToPoint);
                yield break; // 코루틴 간섭 방지를 위해 상태가 변경될 때 일단 코루틴을 중지시키고, 상태가 종료될 때 다시 작동시킨다.
            }

            yield return null;
        }
    }
}
