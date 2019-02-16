using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caminata : MonoBehaviour
{
    public float speed = 2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector2(speed, 0));
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector2(-speed, 0));
        }
    }
}
