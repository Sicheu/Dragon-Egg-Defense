using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using UnityEngine;
using Object = System.Object;

public class NotifyBase
{
    
}

public interface IVMyState
{
    // 현재 상태에게 무언가를 알리려고 할 때 쓰는 API
    void OnNotify<T, T2>(T eValue, T2 vValue) where T : Enum where T2 : NotifyBase;
    
    // 상태가 시작될 때 불리는 API
    public void EnterStateWrapper();
   
    // Update 와 같음
    public void ExcuteStateWrapper();

    // 상태가 끝날 때 불리는 API
    public void ExcuteState_FixedUpdateWrapper();

    // FiexedUpdate
    public void ExcuteState_LateUpdateWrapper();

    // LateUpdate
    public void ExitStateWrapper();

    // 상태를 추가해주는 API
    public void AddState<T>(StateMachine<T> owner, ref object _states) where T : Enum;
}

// 혹시 기능을 확장해야 할 때 쓰려고 여유로 만들어 둠
public abstract class VMyStateBase : MonoBehaviour
{
}

// 상태를 정의하는 추상 클래스. 제네릭 타입 T는 Enum 타입이어야 한다. 상태 기계에 사용되는 상태를 나타냄
public abstract class VMyState<T> : VMyStateBase, IVMyState where T : Enum
{
    public abstract T StateEnum { get; } // 각 상태에서의 Enum 값을 반환 >> 파생 클래스에서 구현하여 각 상태를 구별하게 해줌.
    
    [NonSerialized]public StateMachine<T> OwnerStateMachine; // 이 상태를 소유하는 상태 기계 StateMachine 의 변수
    
    protected virtual void Awake() // 어웨이크가 덮이는 문제를 해결하기 위해 프로텍티드 버츄얼. base.Awake 로 호출해야 한다
    {
    }
    
    protected virtual void Start()
    {
    }
    
    // 이벤트를 처리. 파생클래스에서 구현됨
    public virtual void OnNotify<T1, T2>(T1 eValue, T2 vValue) where T1 : Enum where T2 : NotifyBase
    {
        throw new NotImplementedException();
    }

    public void EnterStateWrapper()
    {
        EnterState();
    }

    public void ExcuteStateWrapper()
    {
        ExcuteState();
    }
    
    public void ExcuteState_FixedUpdateWrapper()
    {
        ExcuteState_FixedUpdate();
    }
    
    public void ExcuteState_LateUpdateWrapper()
    {
        ExcuteState_LateUpdate();
    }

    public virtual void ExitStateWrapper()
    {
        ExitState();
    }
    
    public void AddState<T1>(StateMachine<T1> owner, ref object _states) where T1 : Enum
    {
        var cast =_states as SerializedDictionary<T, IVMyState>;
        OwnerStateMachine = owner as StateMachine<T>;
        cast?.Add(StateEnum, this);
    }

    // !!!실제로 사용자가 사용할 API 5 개
    protected virtual void EnterState()
    {
        
    }

    protected virtual void ExcuteState()
    {
        
    }

    protected virtual void ExitState()
    {
        
    }
    
    protected virtual void ExcuteState_FixedUpdate()
    {
        
    }

    protected virtual void ExcuteState_LateUpdate()
    {
        
    }
    // !!! 여기까지
}

public abstract class HFSMVMyState<T, T2> : VMyState<T2> where T : Enum where T2 : Enum
{
    // HSFM 이용 할 시
    public  StateMachine<T> HSFM_StateMachine;

    public override void ExitStateWrapper()
    {
        base.ExitStateWrapper();
        
        if (HSFM_StateMachine)
        {
            HSFM_StateMachine.ChangeStateNull();
        }
    }
}

// 상태 기계 구현
public class StateMachine<T> : MonoBehaviour where T : System.Enum
{
    [SerializeField] private T defaultState; // 초기 상태 설정, 인스펙터에서 설정할 수 있음
    
    [SerializeField] private IVMyState _currentMyState; // 현재 활성화된 상태
    private SerializedDictionary<T, IVMyState> _states = new(); // Enum 값과 상태 인스턴스를 매핑하는 딕셔너리
    
    StateMachine<T> GetSuperOwnerStateMachine() // 상위 상태 기계를 찾는 메서드. 재귀적으로 반환하며 찾는 형식이며 루트 상태 기계를 찾는다.
    {
        StateMachine<T> stateMachine = GetComponentInParent< StateMachine<T>>();
        if (stateMachine)
        {
            return stateMachine.GetSuperOwnerStateMachine();
        }

        return this;
    }
    
    private void ChangeState_Internal(IVMyState newMyState) // 내부에서 상태 전환
    {
        if (_currentMyState != null)
        {
            _currentMyState.ExitStateWrapper();
        }

        if (newMyState == null)
        {
            _currentMyState = null;
            return;
        }

        _currentMyState = newMyState;
        _currentMyState.EnterStateWrapper();
    }
    
    // 현재 상태에게 특정 이벤트를 알림 = 현재 상태의 OnNotify 매서드에 따라 결과가 달라질 수 있음
    public void OnNotify<T1, T2>(T1 eValue, T2 vValue) where T1 : Enum where T2 : NotifyBase
    {
        _currentMyState.OnNotify(eValue, vValue);
    }
    
    public void ChangeStateNull()
    {
        ChangeState_Internal(null);
    }

    public void ChangeState(T state) // 외부에서 상태 전환 요청할 때 사용
    {
        // 상태가 None이 아니면 돌릴 상태가 있으므로 Active
        if (_states.TryGetValue(state, out var newState))
        {
            ChangeState_Internal(newState);
        }
        else
        {
            Debug.LogError($"State {state} not found in the state machine.");
            ChangeState_Internal(null);
        }
    }

    protected virtual void Awake()
    {
        // 이거는 성능이 직접 컴포넌트 가져오는 방식 대비 비싸다.
        var stateArray = GetComponents<VMyStateBase>().OfType<IVMyState>().ToList(); // 모든 상태를 가져와 배열에 저장
        foreach (var state in stateArray)
        {
            object states = _states;
            state.AddState(this, ref states); // 각 상태의 OwnerStateMachine(이 상태를 소유하는 상태 기계 머신)을 해당 클래스 StateMachine<T> 로 지정 후 설정된 상태를 매핑
        }   
    }

    protected virtual void Start()
    {
        // DefaultState
        ChangeState(defaultState); // 초기 상태 작동
    }
    
    void Update()
    {
        if (_currentMyState != null)
        {
            _currentMyState.ExcuteStateWrapper();
        }
    }
    
    private void FixedUpdate()
    {
        if (_currentMyState != null)
        {
            _currentMyState.ExcuteState_FixedUpdateWrapper();
        }
    }

    private void LateUpdate()
    {
        if (_currentMyState != null)
        {
            _currentMyState.ExcuteState_LateUpdateWrapper();
        }
    }
}
