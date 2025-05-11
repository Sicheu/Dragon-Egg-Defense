using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MonoBehaviour
{
    private Animator _animator;
    public bool Broken = false;
    
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Cracking()
    {
        Broken = true;
        _animator.SetBool("Cracking", Broken);
    }

    public void Regenerating()
    {
        Broken = false;
        _animator.SetBool("Cracking", Broken);
    }
}
