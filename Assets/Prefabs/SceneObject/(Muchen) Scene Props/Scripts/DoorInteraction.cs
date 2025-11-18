using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    private Animator mAnimator;
    private bool isOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        mAnimator = GetComponent<Animator>();
    }

    public void PlayAnimation()
    {
        if(mAnimator !=null)
        {
            if(isOpen)
            {
                mAnimator.SetBool("open", false);

                isOpen = false;
            }
            else
            {
                mAnimator.SetBool("open", true);

                isOpen = true;
            }
        }
    }
}
