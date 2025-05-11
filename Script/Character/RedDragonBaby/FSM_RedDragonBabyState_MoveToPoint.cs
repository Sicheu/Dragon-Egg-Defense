using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// RDB 이동 스크립트. 마우스 Floor 위에서 마우스 오른쪽 클릭 한 곳으로 이동
public class FSM_RedDragonBabyState_MoveToPoint : VMyState<FSM_RedDragonBabyState>
{
    public override FSM_RedDragonBabyState StateEnum => FSM_RedDragonBabyState.FSM_RedDragonBabyState_MoveToPoint;

    private Character_RedDragonBaby _cr;
    private Camera mainCamera;
    private Coroutine moveCoroutine;

    protected override void Awake()
    {
        base.Awake();

        _cr = GetComponent<Character_RedDragonBaby>();
        mainCamera = Camera.main;
    }

    protected override void EnterState()
    {
        _cr._readyToMove = null; // 코루틴 오버플로우 방지를 위한 널처리
        _cr._animator.CrossFade(Character_RedDragonBaby.MoveHash, 0.0f);
    }

    protected override void ExcuteState()
    {
        if (Input.GetMouseButton(1) && _cr._rb != null) // 마우스 오른쪽 버튼을 누르면
        {
            if (!_cr.Selecting) // 이 유닛이 선택중이 아니라면 이동 명령을 수행하지 않음. = 이동 중이라도 체크가 해제되었다면 새로운 이동명령(기존것을 중지하고 새로운 코루틴) 을 받지 않음
            {
                return;
            }
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // 클릭한 곳 카메라 기준으로 레이 쏴서 기억
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _cr.floorLayerMask)) // Floor 레이어 가진 오브젝트에 Ray 가 충돌했다면
            {
                Vector3 targetPosition = hit.point; // 충돌한 지점을 목적지로 저장
                
                if (moveCoroutine != null) // 진행중인 코루틴이 있다면 = 목적지에 도달하기 전에 이동명령이 한 번 더 내려왔다면
                {
                    StopCoroutine(moveCoroutine); // 이전 코루틴 중지
                }
                
                moveCoroutine = StartCoroutine(MoveObject(_cr._rb, targetPosition)); // 목적지로 이동하는 코루틴 작동 or 새로운 목적지로 코루틴 설정
            }
        }
    }

    private IEnumerator MoveObject(Rigidbody obj, Vector3 targetPosition)
    {
        Vector3 currentTarget = targetPosition;
        
        while (Vector3.Distance(obj.position, currentTarget) > 0.1f) // 목적지랑 거리가 0.1 이상인동안
        {
            Vector3 direction = (currentTarget - obj.position).normalized; // 방향 설정
            obj.MovePosition(obj.position + direction * (_cr.Speed * Time.deltaTime)); // 해당 방향으로 이동

            direction.y = 0; // 로테이션 y 값을 고정하여 땅밑을 바라보는 현상을 방지
            _cr._rb.MoveRotation(Quaternion.LookRotation(direction)); // 해당 방향을 바라봄

            if (Input.GetMouseButton(1) && _cr.Selecting) // 코루틴 중에 이동명령이 또 내려온다면 !! 사실 이 if 문이 없어도 기능에 문제는 없음. ExcuteState 에서 이미 코루틴 작동 중에 다른 코루틴 명령이 내려오면 기존것을 중지하게 되어있기 때문
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _cr.floorLayerMask)) // Floor 레이어 가진 오브젝트에 Ray 가 충돌했다면
                {
                    currentTarget = hit.point; // 이동할 목적지를 업데이트
                }
            }
            yield return null;
        }

        moveCoroutine = null; // 이동이 끝난 후 코루틴 변수 값 null
        OwnerStateMachine.ChangeState(FSM_RedDragonBabyState.FSM_RedDragonBabyState_Idle); // 이동이 끝나면 Idle 상태로
    }

    protected override void ExitState()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (_cr.Selecting)
        {
            _cr._readyToMove = StartCoroutine(_cr.ReadyToMove());
        }
    }
}
