using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Stun : ActionBase
{
    private Soldier soldier;
    private Coroutine _coroutine;
    private float StunTime;
    
    public Action_Stun(GameObject owner, float time) : base(owner)
    {
        soldier = owner.GetComponent<Soldier>();
        StunTime = time;
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _coroutine = soldier.StartCoroutine(Stun());
    }

    public override void ExitAction()
    {
        base.ExitAction();

        if (_coroutine != null)
        {
            soldier.StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    IEnumerator Stun()
    {
        yield return new WaitForSeconds(StunTime);
        soldier.OnStunFInish();
        //_coroutine = null;
    }
}
