using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

public class T5_playerController : NetworkBehaviour
{

    private CharacterController controller;

    public float PlayerSpeed = 2f;
    // Start is called before the first frame update
    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
        {
            return;
        }
        
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))* Runner.DeltaTime * PlayerSpeed;
        controller.Move(move);
    }
}
