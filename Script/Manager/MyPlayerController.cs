using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// 오브젝트들을 들고 있으면서, 그 오브젝트들을 생성, 상호작용하는 스크립트
public class MyPlayerController : SceneSingleton<MyPlayerController>
{
    public List<GameObject> Characters = new(); // 캐릭터 프리팹 할당하는 리스트
    private Dictionary<int, GameObject> CharacterInstances = new(); // 생성된 캐릭터 인스턴스의 딕셔너리. 인스턴스 ID 를 키로 사용

    public List<GameObject> SuperiorCharacters = new(); // 상위 캐릭터 프리팹 리스트
    public List<GameObject> HighSuperiorCharacters = new(); // 최상위 캐릭터 프리팹 리스트 

    public List<GameObject> Monsters = new(); // 몬스터 프리팹 할당하는 리스트
    private Dictionary<int, GameObject> MonsterInstances = new(); // 생성된 몬스터 인스턴스의 딕셔너리. 인스턴스 ID 를 키로 사용

    private Dictionary<CharacterType, List<GameObject>> Selecting = new(); // 선택 중인 캐릭터 저장하는 딕셔너리

    // 캐릭터 컨트롤러 로직
    public void SpwanCharacter() // 리스트 내의 캐릭터를 램덤하게 생성한 후, 생성된 오브젝트를 인스턴스로 반환
    {
        GameObject prefab = Characters[Random.Range(0, Characters.Count)];
        GameObject instance = Instantiate(prefab, gameObject.transform.position, Quaternion.identity);
        CharacterInstances.Add(instance.GetInstanceID(), instance); // 이거 왜있지?
    }

    public void SelectSpwanCharacter(int inex) // 선택된 캐릭터를 생성
    {
        GameObject prefab = Characters[inex];
        GameObject instance = Instantiate(prefab, gameObject.transform.position, Quaternion.identity);
        CharacterInstances.Add(instance.GetInstanceID(), instance); // 이거 왜있지?
    }
    
    public void SelectingOnController(ISelectable charater, GameObject obj) // 현재 선택된 유닛들을 딕셔너리에 저장하는 메서드
    {
        CharacterType type = charater.CharacterType; // 이넘 값인 type 은 불러온 유닛의 ISelectable 에 등록된 CharacterType 값으로 설정
        if (!Selecting.ContainsKey(type)) // 딕셔너리에 해당 키 값이 없다면
        {
            Selecting[type] = new List<GameObject>(); // 해당 키를 설정하고 리스트 자리를 만듦
        }
        Selecting[type].Add(obj); // 리스트에 유닛 추가
    }

    public void DeSelectingOnController(ISelectable character, GameObject obj) // 유닛을 선택 해제 할 때 딕셔너리에서 해제하는 메서드
    {
        CharacterType type = character.CharacterType; // 이넘 값인 type 은 불러온 유닛의 ISelectable 에 등록된 CharacterType 값으로 설정
        if (Selecting.TryGetValue(type, out var selectingList)) // 해당 키 값이 딕셔너리에 있다면, 키 값에 해당하는 리스트를 가져옴
        {
            if (selectingList.Contains(obj)) // 리스트에 해당 유닛이 있다면
            {
                selectingList.Remove(obj); // 리스트에서 제거
            }
        }
        else
        {
            Debug.Log("키가 없음");
        }
    }

    public void SpwanSuperiorCharacter() // 조건 충족 시, 상위 종 생성
    {
        TrySpwanSuperiorCharcter(CharacterType.DarkDragonBaby, SuperiorCharacters[0]);
        TrySpwanSuperiorCharcter(CharacterType.GreenDragonBaby, SuperiorCharacters[1]);
        TrySpwanSuperiorCharcter(CharacterType.RedDragonBaby, SuperiorCharacters[2]);
        TrySpwanSuperiorCharcter(CharacterType.DarkDragonAdolescent, HighSuperiorCharacters[0]);
        TrySpwanSuperiorCharcter(CharacterType.GreenDragonAdolescent, HighSuperiorCharacters[1]);
        TrySpwanSuperiorCharcter(CharacterType.RedDragonAdolescent, HighSuperiorCharacters[2]);
    }

    private void TrySpwanSuperiorCharcter(CharacterType characterType, GameObject superiorPrefab) // 해당 유닛들의 캐릭터 타입을 받아와서 타입에 대응하는 상위종 생성
    {
        if (Selecting.TryGetValue(characterType, out var characterList)) // 입력된 CharacterType 키에 맞는 리스트를 가져옴
        {
            while (characterList.Count >= 3) // 리스트 카운트가 3 보다 클 때, 상위종을 생성한다.
            {
                SoundManager.Instance.PlaySound(16);
                for (int i = 0; i < 3; i++) // 3개의 유닛을 리스트에서 제거하고, Destroy
                {
                    GameObject obj = characterList[0];
                    characterList.RemoveAt(0);
                    Destroy(obj);
                }
                Instantiate(superiorPrefab, gameObject.transform.position, Quaternion.identity); // 타입에 맞는 상위종 생성
            }
        }
    }

    public void SellSelectedCharacter()
    {
        foreach (var kvp in Selecting)
        {
            foreach (var obj in kvp.Value)
            {
                Destroy(obj);

                if (kvp.Key == CharacterType.DarkDragonBaby || kvp.Key == CharacterType.GreenDragonBaby || kvp.Key == CharacterType.RedDragonBaby)
                {
                    UiManager.Instance.coin += 50;
                }
                else if (kvp.Key == CharacterType.DarkDragonAdolescent || kvp.Key == CharacterType.GreenDragonAdolescent || kvp.Key == CharacterType.RedDragonAdolescent)
                {
                    UiManager.Instance.coin += 200;
                }
                else if (kvp.Key == CharacterType.DarkDragonAdult || kvp.Key == CharacterType.GreenDragonAdult || kvp.Key == CharacterType.RedDragonAdult)
                {
                    UiManager.Instance.coin += 600;
                }
            }
        }
        
        Selecting.Clear();
    }
    
    // 몬스터 컨트롤러 로직
    public GameObject GetNewMonster(int index,Vector3 position, Quaternion rotation) // 리스트 내의 몬스터를 램덤하게 생성한 후, 생성된 오브젝트를 인스턴스로 반환
    {
        GameObject prefab = Monsters[index];
        GameObject instance = Instantiate(prefab, position, rotation);
        MonsterInstances.Add(instance.GetInstanceID(), instance);
        return instance;
    }
    
    public List<GameObject> GetMonsterList() // 현재 생성된 모든 몬스터 인스턴스를 리스트로 반환. Idle 상태에서 감지 가능한 몬스터 검사를 위해 사용
    {
        return MonsterInstances.Values.ToList();
    }
    
    public void RemoveMonster(int instanceID) // MonsterInstances에서 입력된 값을 제거하는 메서드. 감지 범위에서 벗어났음을 알리기 위해 사용
    {
        if (MonsterInstances.ContainsKey(instanceID))
        {
            MonsterInstances.Remove(instanceID);
        }
    }
}