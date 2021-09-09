using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimController : MonoBehaviour
{
    private Animator animator;
    private int Attack_L1;
    private int Attack_L2;
    private int Attack_L3;

    private void Start()
    {
        animator = GetComponent<Animator>();

        Attack_L1 = Animator.StringToHash("Attack_L1");
        Attack_L2 = Animator.StringToHash("Attack_L2");
        Attack_L3 = Animator.StringToHash("Attack_L3");
    }


    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    animator.SetTrigger(Attack_L1);
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    animator.SetTrigger(Attack_L2);
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    animator.SetTrigger(Attack_L3);
        //}
    }
}
