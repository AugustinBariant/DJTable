using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trigger : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject myPrefab;
    public GameObject clone;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (clone == null)
            {
                clone = (GameObject)Instantiate(myPrefab);
            }
            else
            {
                Destroy(clone);
                clone = (GameObject)Instantiate(myPrefab);
            }
            
        } 
    }
}
