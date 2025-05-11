using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_DarkDragonBabyState_MoveToPoint : VMyState<FSM_DarkDragonBabyState>
{
    public override FSM_DarkDragonBabyState StateEnum => FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_MoveToPoint;
    
    private Character_DarkDragonBaby _cd;
    private Camera mainCamera;
    private Coroutine moveCoroutine;

    protected override void Awake()
    {
        base.Awake();

        _cd = GetComponent<Character_DarkDragonBaby>();
        mainCamera = Camera.main;
    }

    protected override void EnterState()
    {
        _cd._readyToMove = null; // 코루틴 오버플로우 방지를 위한 널처리
        _cd._animator.CrossFade(Character_DarkDragonBaby.MoveHash, 0.0f);
    }

    protected override void ExcuteState()
    {
        if (Input.GetMouseButton(1) && _cd._rb != null) // 마우스 오른쪽 버튼을 누르면
        {
            if (!_cd.Selecting) // 이 유닛이 선택중이 아니라면 이동 명령을 수행하지 않음. = 이동 중이라도 체크가 해제되었다면 새로운 이동명령(기존것을 중지하고 새로운 코루틴) 을 받지 않음
            {
                return;
            }
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // 클릭한 곳 카메라 기준으로 레이 쏴서 기억
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _cd.floorLayerMask)) // Floor 레이어 가진 오브젝트에 Ray 가 충돌했다면
            {
                Vector3 targetPosition = hit.point; // 충돌한 지점을 목적지로 저장

                if (moveCoroutine != null) // 진행중인 코루틴이 있다면 = 목적지에 도달하기 전에 이동명령이 한 번 더 내려왔다면
                {
                    StopCoroutine(moveCoroutine); // 이전 코루틴 중지
                }
                
                moveCoroutine = StartCoroutine(MoveObject(_cd._rb, targetPosition)); // 목적지로 이동하는 코루틴 작동 or 새로운 목적지로 코루틴 설정
            }
        }
    }

    private IEnumerator MoveObject(Rigidbody obj, Vector3 targetPosition)
    {
        Vector3 currentTarget = targetPosition;
        
        while (Vector3.Distance(obj.position, currentTarget) > 0.1f) // 목적지랑 거리가 0.1 이상인동안
        {
            Vector3 direction = (currentTarget - obj.position).normalized; // 방향 설정
            obj.MovePosition(obj.position + direction * (_cd.Speed * Time.deltaTime)); // 해당 방향으로 이동

            direction.y = 0; // 로테이션 y 값을 고정하여 땅밑을 바라보는 현상을 방지
            _cd._rb.MoveRotation(Quaternion.LookRotation(direction)); // 해당 방향을 바라봄

            if (Input.GetMouseButton(1) && _cd.Selecting) // 코루틴 중에 이동명령이 또 내려온다면 !! 사실 이 if 문이 없어도 기능에 문제는 없음. ExcuteState 에서 이미 코루틴 작동 중에 다른 코루틴 명령이 내려오면 기존것을 중지하게 되어있기 때문
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _cd.floorLayerMask)) // Floor 레이어 가진 오브젝트에 Ray 가 충돌했다면
                {
                    currentTarget = hit.point; // 이동할 목적지를 업데이트
                }
            }
            yield return null;
        }

        moveCoroutine = null; // 이동이 끝난 후 코루틴 변수 값 null
        OwnerStateMachine.ChangeState(FSM_DarkDragonBabyState.FSM_DarkDragonBabyState_Idle); // 이동이 끝나면 Idle 상태로
    }

    protected override void ExitState()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (_cd.Selecting) // 이 조건문이 없다면, 이동 완료가 되기 전에 선택 취소를 해버린다면 선택되지 않은 상태에서도 이동 입력 감지 코루틴이 작동되는 버그 발생
        {
            _cd._readyToMove = StartCoroutine(_cd.ReadyToMove()); // 아직 선택 해제가 된것은 아니므로 이동 상태가 끝난 이후라도 언제든지 다시 이동상태로 돌아갈 수 있도록 코루틴 다시 작동 !!! 그렇다고 해당 코루틴을 선택 해제 될때까지 작동시켜놓으면 MoveObject 코루틴과 간섭이 일어나기 때문에 중지시키고, 상태가 시작할때 중지시키고 끝날때 다시 실행시킨다. 
        }
    }
}
