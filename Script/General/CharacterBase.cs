using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 캐릭터가 상태머신과 Rigidbody 를 변수로 가지게 해주고 MonoBehaviour(DefenceBehaviorBase를 통해)를 상속받게 해주는 제너럴 타입 클래스
public class CharacterBase<T> : DefenceBehaviorBase
{
    public T Fsm; // 캐릭터의 상태머신을 참조하는 변수
    public Rigidbody _rb;
    public Animator _animator;
    public Coroutine _readyToMove; // RedayToMove 코루틴을 위한 변수
    
    protected virtual void Awake()
    {
        Fsm = GetComponent<T>();
        _rb = GetComponent<Rigidbody>();
    }
}

public enum CharacterType // 해당 유닛의 타입을 나타내는 이넘
{
    None,
    RedDragonBaby,
    RedDragonAdolescent,
    RedDragonAdult,
    GreenDragonBaby,
    GreenDragonAdolescent,
    GreenDragonAdult,
    DarkDragonBaby,
    DarkDragonAdolescent,
    DarkDragonAdult
}

// 선택박스 영역 내에 캐릭터가 들어왔을 때 발동될 메서드를 인터페이스로 넘겨 GetComponent 유지보수가 쉽게 함
public interface ISelectable
{
    void OnSelected();
    void DeSelected();
    CharacterType CharacterType { get; } // 유닛이 선택 되었을 때 유닛의 타입을 컨트롤러에 알리기 위해 만든 이넘 값
}

public enum EnemyType
{
    None,
    Soldier,
    Tank,
    Plane
}

public enum BossType
{
    Regular,
    Boss
}

public interface IEnemyType
{
    EnemyType EnemyType { get; }
}
