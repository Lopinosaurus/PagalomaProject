using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using Photon.Pun;
using UnityEngine;

public class JumpStateSetter : StateMachineBehaviour
{
    public PlayerMovement.JumpState desiredJumpState;
   
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    // public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //
    // }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var jumpHash in PlayerAnimation.jumpHashes)
        {
            animator.ResetTrigger(jumpHash);
        }
        
        PlayerMovement.SetJumpState(desiredJumpState);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
