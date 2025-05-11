using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
// 그리드 상의 바닥 타일을 관리하고, 플레이어의 마우스 입력에 반응하여 상호작용
public class GridFloor : MonoBehaviour
{
    private Material originMaterial; // 타일의 원래 material 저장 변수
    private Renderer renderer; // 타일의 Renderer 컴포넌트 참조

    [NonSerialized]public GameObject CurrentCharacter; // 이 타일 위에 위치한 캐릭터
    
    void Awake()
    {
        renderer = GetComponent<Renderer>();
        originMaterial = renderer.material; // 타일의 초기 material 저장
    }

    private void OnMouseEnter() // 마우스 커서가 타일 위에 위치했을 떄 호출
    {
        renderer.material = MaterialManager.Instance.outlineMaterial; // 타일의 meterial 을 변경
    }
    
    private void OnMouseExit() // 마우스 커서가 타일에서 벗어났을 때 호출
    {
        renderer.material = originMaterial; // material 초기 설정으로 돌림
    }
    
    // 그리드 단위 이동
    // private void OnMouseDown() // 타일이 클릭 되었을 때 호출
    // {
    //     MyPlayerController.Instance.OnMouseDownFromFloor(this);
    // }
}