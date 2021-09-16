using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorBehaviourScript : StateMachineBehaviour
{
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GDebug.Instance.Log($"OnStateEnter -> Params animator:{animator.ToString()}  stateInfo:{stateInfo.ToString()}  layerIndex:{layerIndex}");
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GDebug.Instance.Log($"OnStateUpdate -> Params animator:{animator.ToString()}  stateInfo:{stateInfo.ToString()}  layerIndex:{layerIndex}");
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GDebug.Instance.Log($"OnStateExit -> Params animator:{animator.ToString()}  stateInfo:{stateInfo.ToString()}  layerIndex:{layerIndex}");
    }

    //OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that processes and affects root motion
        GDebug.Instance.Log($"OnStateMove -> Params animator:{animator.ToString()}  stateInfo:{stateInfo.ToString()}  layerIndex:{layerIndex}");
    }

    //OnStateIK is called right after Animator.OnAnimatorIK()
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that sets up animation IK (inverse kinematics)
        GDebug.Instance.Log($"OnStateIK -> Params animator:{animator.ToString()}  stateInfo:{stateInfo.ToString()}  layerIndex:{layerIndex}");
    }
}
