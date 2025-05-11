using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
// 몬스터1 자신의 데이터들과 핵심 기능들을 구현
public enum Monster1Notify
{
	None,
	Stun
}

public class StunData : NotifyBase
{
	
}

public class Monster1 : CharacterBase<FSM_Monster1>
{
	public float Speed = 5.0f; // 이동 속도
	public int DestinationIndex; // 목적지 인덱스
	public float Hp;
	
    protected override void Awake()
    {
	    base.Awake();
	    DestinationIndex = 0;
	    Hp = 100;
    }

    public void OnStunFInish()
    {
	    Fsm.ChangeState(FSM_Monster1State.FSM_Monster1_LoopingMove);
    }
    
    public void OnStun()
    {
	    Fsm.OnNotify(Monster1Notify.Stun, new StunData());
    }

    public bool MoveToDestination(Vector3 destination) // 몬스터를 목적지로 이동시키고, 목적지에 도달했다면 true 반환
    {
	    Vector3 nextPosition = Vector3.MoveTowards(transform.position, destination, Speed * Time.deltaTime); // MoveTowards 로 현재 위치에서 목적지로 이동할 다음 위치 계산
	    _rb.MovePosition(nextPosition); // 다음 목적지로 MovePosition
	    if (Vector3.Distance(destination, transform.position) <= 1.0f) // 목적지와 현재 위치 사이의 거리가 1.0f 이하면 true 를 반환하여 목적지에 도달하였음을 알림
	    {
		    return true;
	    }
	    _rb.MoveRotation(Quaternion.LookRotation((nextPosition - transform.position).normalized)); // 몬스터가 이동하는 방향을 바라보도록 로테이션 설정

	    return false;
    }
}
