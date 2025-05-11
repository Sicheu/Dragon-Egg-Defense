using System;
using UnityEngine;

public class SelectableCharacter : MonoBehaviour 
{
    public SpriteRenderer selectImage;
    private ISelectable _parent; // ISelectable 인터페이스를 상속받은 모든 캐릭터에 관해 발동할 수 있도록.
    private SelectManager _selectManager;

    private void Awake()
    {
        _selectManager = FindObjectOfType<SelectManager>();
        if (_selectManager != null)
        {
            _selectManager.RegisterSelectableCharacter(this);
        }
        else
        {
            Debug.LogError("SeelectManager 가 씬에 없음");
        }
        
        selectImage.enabled = false;
        _parent = GetComponentInParent<ISelectable>();
    }

    //Turns off the sprite renderer
    public void TurnOffSelector()
    {
        selectImage.enabled = false;
    }

    //Turns on the sprite renderer
    public void TurnOnSelector()
    {
        selectImage.enabled = true;
    }

    // 파괴될 때 SelectManager 에서 자신을 제거
    private void OnDestroy()
    {
        _selectManager.UnregisterSelectableCharacter(this);
        
        _selectManager?.DeslectCharacter(this); // 파괴될 떄 매니저의 현재 선택된 유닛 리스트에서도 제거
    }

    public void OnSelected()
    {
        _parent.OnSelected();
    }

    public void DeSelected()
    {
        _parent.DeSelected();
    }
}
