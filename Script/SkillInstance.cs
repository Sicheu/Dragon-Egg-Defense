using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 스킬의 쿨타임을 관리, Character1 스크립트의 Awake를 통해 할당된 스킬에 붙음
public class SkillInstance : MonoBehaviour
{
    public GameObject target;
    public float Cooltime;
    public SkillInfo info;

    public GameObject[] targets;

    public bool IsCooltiming()
    {
        return Cooltime > 0.0f;
    }

    public void StartCooltime()
    {
        StartCoroutine(StartCooltime_Internal());
    }

    IEnumerator StartCooltime_Internal()
    {
        Cooltime = info.Cooltime;
        while (Cooltime > 0.0f)
        {
            Cooltime -= Time.deltaTime;
            yield return null;
        }
    }
}