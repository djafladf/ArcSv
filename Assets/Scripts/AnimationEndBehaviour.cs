using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AnimationEndBehaviour : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.SetActive(false);
    }

}