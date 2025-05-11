using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 스킬인포 생성하는 스크립트
[CreateAssetMenu(fileName = "SkillInfo", menuName = "Skill/CreateSkillInfo")]
public class SkillInfo : ScriptableObject
{
    public float AttackDistance;
    public float Cooltime;
    public float Damage;
    public float StunTime;
    public float MultipeTargetCount;

    public string AnimationName;
    
    [NonSerialized] public int AnimationName_Hash;

    void OnEnable()
    {
        AnimationName_Hash = Animator.StringToHash(AnimationName);
    }
}
