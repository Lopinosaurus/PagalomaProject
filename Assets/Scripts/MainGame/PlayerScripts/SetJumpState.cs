using System;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class SetJumpState : StateMachineBehaviour
    {
        [SerializeField] private PlayerMovement.JumpState desiredSetState = PlayerMovement.JumpState.Still;
        public PlayerMovement playerMovement;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (null != playerMovement)
            {
                playerMovement.SetJumpState(desiredSetState);
            }
            else
            {
                try
                {
                    playerMovement = RoomManager.Instance.localPlayer.gameObject.GetComponent<PlayerMovement>();
                }
                catch
                {
                    Debug.LogWarning("No localPlayerFound !");
                }
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        // public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        // {
        // }

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
}