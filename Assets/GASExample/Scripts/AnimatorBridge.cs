using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorBridge : MonoBehaviour
{
    private Animator _animator;
    public Action<Animator> OnAnimatorMoveCall { get; set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        OnAnimatorMoveCall?.Invoke(_animator);
    }
    
    public void SetBool(string name, bool value)
    {
        if (_animator == null) return;
        _animator.SetBool(name, value);
    }
}
