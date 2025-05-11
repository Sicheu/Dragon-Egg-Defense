using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
// 몬스터1 자신의 데이터들과 핵심 기능들을 구현
public enum SoldierNotify
{
	None,
	Stun
}

public class StunData_Soldier : NotifyBase
{
	public float StunTime;

	public StunData_Soldier(float stunTime)
	{
		StunTime = stunTime;
	}
}

public class Soldier : CharacterBase<FSM_Soldier>, IEnemyType
{
	public EnemyType enemyType; // 인스펙터에서 타입 지정
	public EnemyType EnemyType => enemyType; // 지정한 타입을 가져와서 해당 오브젝트의 타입으로 삼음
	public BossType bossType;
	public BossType BossType => bossType;
	
	public float Speed = 5.0f; // 이동 속도
	public int DestinationIndex; // 목적지 인덱스
	[NonSerialized]public float Hp = 100;
	public float Defende;
	
	public readonly int WalkHash = Animator.StringToHash("Walking");
	public readonly int DeadHash = Animator.StringToHash("Dead");
	public readonly int StunHash = Animator.StringToHash("Stun");

	[NonSerialized] public bool isDead = false; // 죽었음을 알리는 변수

	public float StunTime; // 입력받은 스턴시간을 저장할 변수
	public bool isStunned { get; private set; } // 외부에 스턴 중임을 알리는 변수. 프로퍼티로 외부에서 읽기만 가능하고 설정은 내부에서만 가능하게 함
	
	private bool immuneStun = false;
	private Coroutine immuneStunCoroutine;
	public float immuneStunTime;

	[NonSerialized] public GameObject stunEffectGameObject; // 스턴 파티클을 저장할 변수

	protected override void Awake()
    {
	    base.Awake();
	    
	    _animator = GetComponentInChildren<Animator>();

	    DestinationIndex = 0;

	    isStunned = false;

	    if (BossType == BossType.Boss)
	    {
		    Hp = 300;
	    }
    }
    public void OnStunFInish()
    {
	    isStunned = false;
	    Fsm.ChangeState(FSM_SoldierState.FSM_Soldier_LoopingMove);

	    if (BossType == BossType.Boss)
	    {
		    Debug.Log($"{immuneStunTime}간 스턴면역");
		    immuneStunCoroutine = StartCoroutine(ImmuneTime());
	    }
    }
    
    public void OnStun(float stunTime)
    {
	    if (BossType == BossType.Regular)
	    {
		    isStunned = true;
		    Fsm.OnNotify(SoldierNotify.Stun, new StunData_Soldier(stunTime));
	    }
	    else if (BossType == BossType.Boss)
	    {
		    if (!immuneStun)
		    {
			    if (immuneStunCoroutine != null)
			    {
				    StopCoroutine(immuneStunCoroutine);
				    immuneStunCoroutine = null;
			    }
			    immuneStun = true;
			    isStunned = true;
			    Fsm.OnNotify(SoldierNotify.Stun, new StunData_Soldier(stunTime));
		    }
		    else
		    {
			    Debug.Log("스턴 면역 상태임");
		    }
	    }
    }

    public bool MoveToDestination(Vector3 destination) // 몬스터를 목적지로 이동시키고, 목적지에 도달했다면 true 반환
    {
	    Vector3 nextPosition = Vector3.MoveTowards(transform.position, destination, Speed * Time.deltaTime); // MoveTowards 로 현재 위치에서 목적지로 이동할 다음 위치 계산
	    _rb.MovePosition(nextPosition); // 다음 목적지로 MovePosition

	    Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.z);
	    Vector2 destination2D = new Vector2(destination.x, destination.z);
	    
	    if (Vector3.Distance(currentPos2D, destination2D) <= 1.0f) // 목적지와 현재 위치 사이의 거리가 1.0f 이하면 true 를 반환하여 목적지에 도달하였음을 알림
	    {
		    return true;
	    }
	    
	    // y 축을 무시한 방향을 계산 !! 목적지에 도달했을 때 솔저가 위나 아래를 보는 현상을 방지
	    Vector3 direction = new Vector3(destination.x - transform.position.x, 0, destination.z - transform.position.z);
    
	    if (direction != Vector3.zero) // 방향이 0이 아닐 때만 회전
	    {
		    Quaternion rotation = Quaternion.LookRotation(direction.normalized);
		    _rb.MoveRotation(rotation); // 몬스터가 이동하는 방향을 바라보도록 로테이션 설정
	    }
	    
	    // if (Vector3.Distance(destination, transform.position) <= 1.0f) // 목적지와 현재 위치 사이의 거리가 1.0f 이하면 true 를 반환하여 목적지에 도달하였음을 알림
	    // {
		   //  return true;
	    // }
	    // _rb.MoveRotation(Quaternion.LookRotation((nextPosition - transform.position).normalized)); // 몬스터가 이동하는 방향을 바라보도록 로테이션 설정

	    return false;
    }
    
    public void NotifyRedDragonBabyOnDeath() // 반경 내 해당 오브젝트를 타겟으로 하는 드래곤의 타겟 해제 및 인스턴스 제거
    {
	    // 몬스터 인스턴스에서 제거 !! 캐릭터가 다시 Idle 로 돌아가 타겟을 찾을 때 해당 Dead 애니메이션 중인 해당 오브젝트를 인식하지 않도록
	    MyPlayerController.Instance.RemoveMonster(gameObject.GetInstanceID()); 
	    
	    // 타겟된 드래곤들한테서 타겟팅 제거
	    int targetLayer = LayerMask.NameToLayer("Dragon"); // 특정 레이어를 위한 레이어 설정
	    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(); // 씬의 모든 오브젝트 탐색

	    foreach (var obj in allObjects)
	    {
		    if (obj.layer == targetLayer)
		    {
			    Character_RedDragonBaby redDragonBaby = obj.GetComponent<Character_RedDragonBaby>();
			    if (redDragonBaby != null &&  redDragonBaby.ActiveSkillInstance != null && redDragonBaby.ActiveSkillInstance.target == gameObject)
			    {
				    redDragonBaby.ClearTarget();
			    }
		    }
	    }
    }

    public void Dameged(float damage)
    {
	    float newHp = Hp - (damage / Defende);
	    Hp = newHp;
    }

    public void LoopingMoveComplete()
    {
	    NotifyRedDragonBabyOnDeath();
	    Destroy(gameObject);

	    if (BossType == BossType.Regular)
	    {
		    UiManager.Instance.GetDamaged(1);
	    }
	    else if (BossType == BossType.Boss)
	    {
		    UiManager.Instance.GetDamaged(10);
	    }
    }

    private IEnumerator ImmuneTime()
    {
	    yield return new WaitForSeconds(immuneStunTime);
	    immuneStun = false;
    }
}
