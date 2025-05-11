using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionBase
{
    private GameObject _owner;

    public ActionBase(GameObject owner)
    {
        _owner = owner;
    }

    public virtual void EnterAction()
    {
        
    }

    public virtual void UpdateteAction()
    {
        
    }

    public virtual void ExitAction()
    {
        
    }

    public virtual void FixedUpddateAction()
    {
        
    }

    public virtual void LateUpddateAction()
    {
        
    }
}
