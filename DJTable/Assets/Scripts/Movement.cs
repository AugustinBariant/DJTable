using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public Rigidbody2D Rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("d")) {
            Vector2 v2;
            v2 = new Vector2(1000f*Time.deltaTime, 0f);
            Rb.AddForce(v2);
        }

        if (Input.GetKey("q"))
        {
            Vector2 v2;
            v2 = new Vector2(-1000f * Time.deltaTime, 0f);
            Rb.AddForce(v2);
        }

        if (Input.GetKey("z"))
        {
            Vector2 v2;
            v2 = new Vector2(0f,1000f * Time.deltaTime);
            Rb.AddForce(v2);
        }
    }
}
