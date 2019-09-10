using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerPosition;
    public Transform cameraPosition;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position.Set(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPosition.position.x > 5) {
            Debug.Log("heee");
        }
        Vector3 offSet = new Vector3(0f, 0f, -10f);
        cameraPosition.position = playerPosition.position + offSet;
    }
}
