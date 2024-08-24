using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;
using UnityEditor.UI;

public class TrafficLightResponder : MmBaseResponder
{
    public bool direction1 = true;

    Renderer objRenderer;

    public int mode;

    float delay;
    // Start is called before the first frame update
    public override void Start()
    {
        objRenderer = GetComponent<Renderer>();

        mode = 0;
        SetLightColorMaterialOffset(new Vector2(0.0f, 0.0f));
        if(direction1)
        {
            mode = 2;
            SetLightColorMaterialOffset(new Vector2(0.0625f * 2.0f, 0.0f));
        }
        delay = 2.0f;
    }

    private void SetLightColorMaterialOffset(Vector2 vector2)
    {
        // objRenderer.materials[1].SetTextureOffset("_MainTex", vector2);
        List<Material> materials = new List<Material>();
        GetComponent<Renderer>().GetSharedMaterials(materials);
        materials[1].SetTextureOffset("_MainTex", vector2);
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i].name != "ampeln_de_4k-main" || materials[i].name != "ampeln_de_4k-lights")
            {
                materials.RemoveAt(i);
                i--;
            }
        }
    }

    // Update is called once per frame
    public override void Update()
    {

        if(mode == 0)
        {    
            delay -= Time.deltaTime;
            if(delay < 0.0f)
            {
                delay = 1.0f;
                SetLightColorMaterialOffset(new Vector2(0.0625f * 1.0f, 0.0f));                
                mode = 1;
            }
        }
        else if(mode == 1)
        {    
            delay -= Time.deltaTime;
            if(delay < 0.0f)
            {
                delay = 5.0f;
                SetLightColorMaterialOffset(new Vector2(0.0625f * 2.0f, 0.0f));                
                mode = 2;
            }
        }
        else if(mode == 2)
        {    
            delay -= Time.deltaTime;
            if(delay < 0.0f)
            {
                delay = 1.0f;
                SetLightColorMaterialOffset(new Vector2(0.0625f * 3.0f, 0.0f));                
                mode = 3;
            }
        }
        else if(mode == 3)
        {    
            delay -= Time.deltaTime;
            if(delay < 0.0f)
            {
                delay = 5.0f;
                SetLightColorMaterialOffset(new Vector2(0.0625f * 0.0f, 0.0f));                
                mode = 0;
            }
        }

        base.Update();
    }

    public override void SetActive(bool activeState)
    {
        Debug.Log("TrafficLightResponder: SetActive: " + activeState);
        if(activeState && direction1)
        {
            // turn on the green light
            delay = 5.0f;
            SetLightColorMaterialOffset(new Vector2(0.0625f * 2.0f, 0.0f));
            mode =2;
        }
        else if(activeState ==false && direction1)
        {
            // turn off the gree light
            
            delay = 5.0f;
            SetLightColorMaterialOffset(new Vector2(0.0625f * 0.0f, 0.0f));
            mode = 0;
        }
        else if(activeState && !direction1)
        {
            //turn off the green light
            
            delay = 5.0f;
            SetLightColorMaterialOffset(new Vector2(0.0625f * 0.0f, 0.0f));
            mode = 0;
        }
        else if(activeState == false && !direction1)
        {
            // turn on the green light
            delay = 5.0f;
            SetLightColorMaterialOffset(new Vector2(0.0625f * 2.0f, 0.0f));
            mode = 2;
        }
    }
}
