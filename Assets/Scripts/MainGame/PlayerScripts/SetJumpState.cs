using System;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class SetJumpState : StateMachineBehaviour
    {
        [SerializeField] private PlayerMovement.JumpState desiredSetState = PlayerMovement.JumpState.Still;
        public PlayerMovement PlayerMovement;

        private void Awake()
        {
            PlayerMovement = RoomManager.Instance.localPlayer.gameObject.GetComponent<PlayerMovement>();
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (null != PlayerMovement) PlayerMovement.SetJumpState(desiredSetState);
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
