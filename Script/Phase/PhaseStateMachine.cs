using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PhaseState // 페이즈 전반에 거루어져 다뤄질 이넘값
{
    NoneMyState,
    Phase1_Ready,
    Phase1_Running
}

// StateMachine 제너릭을 사용하여 만든 페이즈상태머신
public class PhaseStateMachine : StateMachine<PhaseState>
{
}