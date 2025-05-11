/*************************************************************
 * Created by Sun Dryd Studios                               *
 * For the Unity Asset Store                                 *
 * This asset falls under the "Creative Commons License"     *
 * For support email sundrysdtudios@gmail.com                *
 *************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SelectManager : MonoBehaviour
{
    public Camera selectCam; // 유닛선택에 사용될 카메라
    public RectTransform SelectingBoxRect; // 유닛을 선택할 때 화면에 표시되는 녹색 사각형 이미지의 RectTransform

    private Rect SelectingRect; // 선택 영역을 나타내는 Rect 구조체
    private Vector3 SelectingStart; // 마우스 처음 클릭한 위치 저장 변수

    public float minBoxSizeBeforeSelect = 10f; // 드래그하여 유닛을 선택할 때, 선택 박스가 해당 최소크기 변수보다 작으면 선택이 이루어지지않음
    public float selectUnderMouseTimer = 0.1f; // 마우스 클릭 후 캐릭터를 선택할 때의 지연 시간
    private float selectTimer = 0f; // 선택 타이머, 클릭 후 시간이 얼마나 지났는지 측정

    private bool selecting = false; // 드래그 중인지 확인하는 변수
    private bool clicked; // 유닛 선택 해제할때, GetButton 변수를 사용하기 때문에 여러번 반복되지 않게 하기 위한 변수

    public List<SelectableCharacter>
        selectableChars = new List<SelectableCharacter>(); // 선택 가능한 모든 캐릭터의 리스트(해당 T 의 스크립트를 가지고 있는지)

    private List<SelectableCharacter> selectedArmy = new List<SelectableCharacter>(); // 현재 선택된 캐릭터들을 저장하는 리스트

    public LayerMask UI_Button; // UI 버튼에서 유닛 선택 기능이 작동하지 않게 설정하는 변수

    private void Awake()
    {
        if (!SelectingBoxRect) // 변수에 참조된 RectTransform 이 없다면, 가져오기
        {
            SelectingBoxRect = GetComponent<RectTransform>();
        }

        SelectableCharacter[]
            chars = FindObjectsOfType<SelectableCharacter>(); // SelectableCharacter 스크립트를 가진 모든 오브젝트를 배열로 가져옴
        for (int i = 0; i <= (chars.Length - 1); i++)
        {
            selectableChars.Add(chars[i]); // 배열의 모든 요소를 리스트에 저장
        }
    }

    // 게임 진행 중에 씬에 추가된 오브젝트 또한 선택 가능한 캐릭터 리스트에 추가하기 위한 메서드
    public void RegisterSelectableCharacter(SelectableCharacter character)
    {
        if (!selectableChars.Contains(character))
        {
            selectableChars.Add(character);
        }
    }
    
    // 게임 진행 중 오브젝트가 제거되었을 때 그 오브젝트를 리스트에서 제거하기 위한 메서드
    public void UnregisterSelectableCharacter(SelectableCharacter character)
    {
        if (selectableChars.Contains(character))
        {
            selectableChars.Remove(character);
        }
    }

    void Update()
    {
        if (SelectingBoxRect == null) // 만일의 경우, 오브젝트에 RectTransform 이 없을 경우를 방지
        {
            Debug.LogError("There is no Rect Transform to use for selection!");
            return;
        }

        // !!!!! 감지하기 위해 만들어지는 선택 영역 초록 박스 조차 UI 이기 때문에, 마우스가 UI 요소 위에 있는지 검사하는 아래 로직은 성능 문제와 충돌 문제로 버그를 일으킴
        // // 현재 마우스가 UI 요소 위에 있는지 확인하여, 아래 로직을 실행하지 않도록 한다.
        // if (EventSystem.current.IsPointerOverGameObject())
        // {
        //     Debug.Log("리턴");
        //     return;
        // }
        
        if (OnUIButton(UI_Button)) // 변수에서 설정된 레이어 위에 있을 때만 아래 로직을 실행하지 않음
        {
            return;
        }

        //The input for triggering selecting. This can be changed
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl)) // 레프트 컨트롤과 같이 누를때, 단일 동시 선택이 가능하게
        {
            clicked = true;
            
            SelectingStart = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0); // 초기 위치 저장
            SelectingBoxRect.anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y); // 초기 앵커 위치 저장
        }
        else if (Input.GetMouseButtonDown(0))
        {
            clicked = true;
            
            ReSelect(); // 현재 선택된 유닛을 초기화

            //Sets up the screen box
            SelectingStart = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0); // 초기 위치 저장
            SelectingBoxRect.anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y); // 초기 앵커 위치 저장
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selectTimer = 0f; // 타이머 초기화
        }

        selecting = Input.GetMouseButton(0); // selecting 변수를 "드래그 중이라면" 으로 설정

        if (selecting)
        {
            SelectingArmy(); // 선택 박스 생성 밑 영역 측정
            selectTimer += Time.deltaTime;

            // 마우스 드래그 시간이 매우 짧을때, 즉, 단일 유닛 선택일 때
            if (selectTimer <= selectUnderMouseTimer && clicked)
            {
                clicked = false;
                CheckIfUnderMouse();
            }
        }
        else
        {
            SelectingBoxRect.sizeDelta = new Vector2(0, 0);
        }
    }

    //Resets what is currently being selected
    void ReSelect() // 현재 선택된 캐릭터가 등록되어 있는 리스트를 초기화
    {
        // 이 기존 코드는, for 문이 돌아가면서 Remove 를 통해 리스트의 Count 가 지속적으로 감소하기 때문에 리스트를 전부 비우는것이 불가능한 문제가 있었음.
        // for (int i = 0; i < selectedArmy.Count; i++)
        // {
        //     selectedArmy[i].TurnOffSelector(); // 초록 원 이미지를 끄고
        //     GetDeSelected(selectedArmy[i]);
        //     selectedArmy.Remove(selectedArmy[i]); // 리스트에서 제거
        //     Debug.Log($"{i} 초기화");
        // }

        while (selectedArmy.Count > 0)
        {
            if (selectedArmy[0] != null) // 상위 종 생성에서 Destroy 되었을 때 값이 null 이 되는 것을 방지 !! SelectableCharacter 스크립트에서 OnDestroy 메서드로 제거되기는 하나, 만일을 대비하여 방지
            {
                selectedArmy[0].TurnOffSelector();
                GetDeSelected(selectedArmy[0]);
            }
            
            selectedArmy.RemoveAt(0);
        }
    }

    //Does the calculation for mouse dragging on screen
    //Moves the UI pivot based on the direction the mouse is going relative to where it started
    //Update: Made this a bit more legible
    void SelectingArmy() // 선택 박스를 생성, 선택 영역을 계산
    {
        // 마우스 드래그에 따른 박스 위치와 크기 설정에 사용될 변수 선언
        Vector2 _pivot = Vector2.zero;
        Vector3 _sizeDelta = Vector3.zero;
        Rect _rect = Rect.zero;

        // x축 방향 피봇 및 크기 조정
        if (-(SelectingStart.x - Input.mousePosition.x) > 0) // 마우스가 오른쪽으로 드래그 되면
        {
            _sizeDelta.x = -(SelectingStart.x - Input.mousePosition.x); // _sizeDelta.x 양수 설정
            _rect.x = SelectingStart.x; // 시작위치 x 를 기반으로 rect x 축 위치 설정
        }
        else // 왼쪽으로 드래그 되면
        {
            _pivot.x = 1; // 피봇을 오른쪽으로 설정
            _sizeDelta.x = (SelectingStart.x - Input.mousePosition.x); // _sizeDelta.x 를 음수로 설정하여 박스의 크기를 반전시킴
            _rect.x = SelectingStart.x - SelectingBoxRect.sizeDelta.x;
        }

        // y 축 방향 피봇 및 크기 조정
        if (SelectingStart.y - Input.mousePosition.y > 0)
        {
            _pivot.y = 1;
            _sizeDelta.y = SelectingStart.y - Input.mousePosition.y;
            _rect.y = SelectingStart.y - SelectingBoxRect.sizeDelta.y;
        }
        else
        {
            _sizeDelta.y = -(SelectingStart.y - Input.mousePosition.y);
            _rect.y = SelectingStart.y;
        }

        // 계산된 피봇 적용
        if (SelectingBoxRect.pivot != _pivot)
            SelectingBoxRect.pivot = _pivot;

        // 계산된 크기 적용
        SelectingBoxRect.sizeDelta = _sizeDelta;

        // Rect 의 크기를 설정된 선택 박스 크기로 설정하고, 값을 변수에 저장
        // 기존 코드는 height 와 width 의 설정이 반대로 되어 있었음
        // _rect.height = SelectingBoxRect.sizeDelta.x;
        // _rect.width = SelectingBoxRect.sizeDelta.y;
        _rect.width = SelectingBoxRect.sizeDelta.x;
        _rect.height = SelectingBoxRect.sizeDelta.y;
        SelectingRect = _rect;

        // 선택 박스의 크기가 최소 크기 보다 큰 경우 유닛 선택 검사 수행 !! 단순 클릭과 드래그를 구분하기 위함
        if (_rect.height > minBoxSizeBeforeSelect && _rect.width > minBoxSizeBeforeSelect)
        {
            CheckForSelectedCharacters();
        }
    }

    //Checks if the correct characters can be selected and then "selects" them
    void CheckForSelectedCharacters() // 선택 박스 안에 포함된 유닛들을 확인하고, 해당 유닛을 선택하거나 선택해제함
    {
        foreach (SelectableCharacter soldier in selectableChars) // 선택 가능한 모든 캐릭터 검사
        {
            Vector2 screenPos =
                selectCam.WorldToScreenPoint(soldier.transform.position); // 유닛의 월드 좌표를 화면 좌표로 변환하여 변수에 할당
            if (SelectingRect.Contains(screenPos)) // 유닛이 선택박스 영역 안이라면
            {
                if (!selectedArmy.Contains(soldier)) // 유닛이 선택된 캐릭터 리스트에 포함되어 있지 않다면
                    selectedArmy.Add(soldier); // 리스트에 추가

                soldier.TurnOnSelector(); // 해당 유닛의 초록색 원 활성화
                GetSelected(soldier);
            }
            else if (!SelectingRect.Contains(screenPos)) // 유닛이 선택박스 밖이라면
            {
                soldier.TurnOffSelector(); // 해당 유닛 초록색 원 비활성화

                if (selectedArmy.Contains(soldier)) // 선택 캐릭터 리스트에 포함되어 있던 유닛이라면
                {
                    GetDeSelected(soldier);
                    selectedArmy.Remove(soldier); // 리스트에서 제거
                }
            }
        }
    }

    //Checks if there is a character under the mouse that is on the Selectable list
    void CheckIfUnderMouse() // 클릭이나, 드래그 범위가 미니멈보다 적을때 단일 유닛을 선택하는 메서드
    {
        RaycastHit hit;
        Ray ray = selectCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.transform != null)
            {
                SelectableCharacter selectChar = hit.transform.gameObject.GetComponentInChildren<SelectableCharacter>();
                if (selectChar != null && selectedArmy.Contains(selectChar)) // 이미 선택된 유닛을 골랐을 경우
                {
                    // 그 유닛을 선택 해제한다
                    selectedArmy.Remove(selectChar);
                    selectChar.TurnOffSelector();
                    GetDeSelected(selectChar);
                }
                else if (selectChar != null && selectableChars.Contains(selectChar)) // 선택된 유닛이 아니고, 선택가능한 리스트에 있을 경우 선택
                {
                    selectedArmy.Add(selectChar);
                    selectChar.TurnOnSelector();
                    GetSelected(selectChar);
                }
            }
        }
    }

    // 선택되었음을 해당 캐릭터에게 알리고, 관련 기능들을 실행시킴
    private void GetSelected(SelectableCharacter selectChar)
    {
        if (selectChar != null)
        {
            selectChar.OnSelected();
        }
    }
    
    // 선택을 해제 시키는 메서드를 불러옴
    private void GetDeSelected(SelectableCharacter selectChar)
    {
        if (selectChar != null)
        {
            selectChar.DeSelected();
        }
    }

    // 마우스 커서가 특정 레이어에 속한 UI 버튼 위에 있는지를 확인하는 기능. UI 버튼을 누를때 선택된 유닛이 선택 해제되는 것을 방지
    private bool OnUIButton(LayerMask layerMask)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current); // PointEventData = UI 이벤트 시스템에서 마우스, 터치 등에 대한 데이터를 나타냄. EventSystem.current 는 현재 활성화된 이벤트 시스템을 참조한다는 것을 의미
        pointerData.position = Input.mousePosition; // 마우스 현재 위치를 할당

        List<RaycastResult> results = new List<RaycastResult>(); // 레이캐스트 결과를 저장할 리스트 생성
        EventSystem.current.RaycastAll(pointerData, results); // pointerData 에 저장된 마우스 위치를 기준으로 레이캐스트를 수행해 해당 위치의 모든 UI 요소를 리스트에 저장

        foreach (var result in results)
        {
            if (layerMask == (layerMask | (1 << result.gameObject.layer))) // 비트연산자를 사용해 layerMask 와 해당 UI 오브젝트가 가진 레이어가 같다면
            {
                return true;
            }
        }

        return false;
    }

    public void DeslectCharacter(SelectableCharacter character) // 캐릭터가 파괴될 때 선택 중인 유닛 리스트에서 제거
    {
        if (selectedArmy.Contains(character))
        {
            selectedArmy.Remove(character);
        }
    }
}
