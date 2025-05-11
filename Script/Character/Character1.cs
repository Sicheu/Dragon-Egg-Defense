using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 캐릭터1 값 및 기능
public class Character1 : CharacterBase<FSM_Character1>
{
    public List<SkillInfo> SkillInfos; // 캐릭터가 사용할 수 있는 스킬의 정보 리스트
    public Vector3 Destination; // 캐릭터가 이동할 목적지 변수

    public List<SkillInstance> skillInstances; // 각 스킬의 인스턴스 리스트
    public SkillInstance activeSkillInstance; // 현재 활성화된 스킬 인스턴스

    public float Speed = 5.0f; // 캐릭터 이동 속도

    public static readonly Vector3 addtiveSpawnVector = Vector3.up * 10; // 스폰 위치 조정을 위한 상수
    public static readonly int IdleHash = Animator.StringToHash("Rider_zombie_Idle1"); 
    public static readonly int runHash = Animator.StringToHash("Rider_zombie_Run");
    
    protected override void Awake()
    {
        base.Awake();

        foreach (var skillInfo in SkillInfos) // 할당한 스킬인포의 리스트를 포이치문
        {
            SkillInstance inst = gameObject.AddComponent<SkillInstance>(); // 새로운 스킬인스턴스 컴포넌트(쿨타임 관리 컴포넌트) 불러옴
            inst.info = skillInfo; // 새로 불러온 스킬 인스턴스의 info 로 클래스가 참조하는 스킬인포를 할당
            skillInstances.Add(inst); // 스킬 인포가 설정된 스킬 인스턴트를 리스트에 저장
        }

        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    public void StartSkillInstance(SkillInstance instance) // 특정 스킬 인스턴스를 활성화하고 상태를 변경하는 메서드. !! 지금은 스킬 하나라 상태 하나로 밖에 못바꾸게 설정하신듯?
    {
        activeSkillInstance = instance;
        Fsm.ChangeState(FSM_Character1State.FSM_Character1State_Skill1);
    }

    public void SetDestination(Vector3 destination) // 캐릭터의 이동 목적지를 설정하고 상태를 이동상태로 변경
    {
        Vector3 rayStart = destination + addtiveSpawnVector; // 레이캐스트의 시작 위치 설정. 목적지가 그리드 좌표로 되있기 때문에 y로 10 정도를 더해준다
        Vector3 rayEnd = Vector3.down; // 레이케스트의 방향 설정

        RaycastHit rh; // 레이케스트가 충돌한 정보 저장
        int layerMask = 1 << LayerMask.NameToLayer("Default"); //layerMask는 "Default" 레이어에 대해서만 레이캐스트가 작동하도록 설정. 1 << LayerMask.NameToLayer("Default")는 "Default" 레이어의 비트 시프트 값을 계산
        if (Physics.Raycast(rayStart, rayEnd, out rh, 1000, layerMask)) // rayStart 에서 rayEnd 방향으로 maxDistance 1000 만큼 레이케스트를 수행, layerMask 에서 설정된 레이어 오브젝트에 충돌하면 true 를 반환하고, 충돌 지점 정보를 rh 에 저장
        {
            Destination = rh.point; // 충돌한 지점의 위치를 Destination 변수에 저장 = 목적지로 설정
        }
        
        Fsm.ChangeState(FSM_Character1State.FSM_Character1State_MoveToDestination); // 상태를 이동 상태로 변환
    }
}