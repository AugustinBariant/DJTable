using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scintillate : MonoBehaviour
{

    public Material m;
    private int activationState;
    // Start is called before the first frame update
    void Start()
    {
        m.color = Color.green;
        activationState = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        activationState = activationState < 2 ? activationState + 1 : 0;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (activationState == 1)
        {
            m.color = Color.red;
        }
        else if (activationState == 2)
            {
            m.color = Color.blue;
        }
        else{
            m.color = Color.green;
        }
    }
}
