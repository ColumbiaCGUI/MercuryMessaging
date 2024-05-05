using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;

public class CandleHandler : MmBaseResponder
{
    [SerializeField] GameObject fireParticle;
    public override void MmInvoke(MmMessage message)
    {

        var type = message.MmMethod;

        switch (type)
        {
            case ((MmMethod)CandleMessageMethod.UpdateLit):
                bool interacted = ((CandleMessage)message).interacted;
                UpdateLit(interacted);
                break;
            default:
                base.MmInvoke(message);
                break;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void UpdateLit(bool interacted)
    {
        bool isLit = fireParticle.activeSelf;
        if (isLit)
        {
            fireParticle.SetActive(false);
        }
        else
        {
            fireParticle.SetActive(true);
        }
    }
}
