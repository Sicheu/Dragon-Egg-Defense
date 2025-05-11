using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Nest : SimpleSingleton<Nest>
{
    public List<GameObject> Eggs = new List<GameObject>(); // 참조할 리스트
    private List<GameObject> UnBrokenEggs = new List<GameObject>(); // 아직 부숴지지 않은 알
    private List<GameObject> BrokenEggs = new List<GameObject>(); // 부숴진 알
    
    void Awake()
    {
        UnBrokenEggs.AddRange(Eggs); // Eggs 에 등록된 모든 값 복사
    }

    public void GetDamaged(int damage) // 데미지를 입힌 수 만큼 알을 파괴하는 메서드
    {
        for (int i = 0; i < damage; i++)
        {
            if (UnBrokenEggs.Count > 0)
            {
                int randomIndex = Random.Range(0, UnBrokenEggs.Count);
                GameObject egg = UnBrokenEggs[randomIndex];

                Egg eggComponent = egg.GetComponent<Egg>();
                eggComponent.Cracking();

                UnBrokenEggs.Remove(egg);
                BrokenEggs.Add(egg);
            }
            else
            {
                Debug.Log("이미 모든 알이 파괴 되었습니다");
            }
        }
    }

    public void RegeneratingLifePoint() // 알을 회복하는 메서드
    {
        if (BrokenEggs.Count > 0)
        {
            int randomIndex = Random.Range(0, BrokenEggs.Count);
            GameObject brokenEgg = BrokenEggs[randomIndex];

            Egg brokenEggComponent = brokenEgg.GetComponent<Egg>();
            brokenEggComponent.Regenerating();

            BrokenEggs.Remove(brokenEgg);
            UnBrokenEggs.Add(brokenEgg);
        }
    }
}
