using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caminata1 : MonoBehaviour
{


    public float speed = 2f;
    private bool der = false;
    private bool izq = false;
    int cont = 1;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

        if (Input.GetKey(KeyCode.D))
        {
           
            transform.Translate(new Vector2(speed, 0));

            if (!der)
            {


                transform.position = new Vector3(gameObject.transform.position.x + 2.29f, gameObject.transform.position.y, gameObject.transform.position.z);
                der = true;
            }
           
            izq = false;
        }
     

        if (Input.GetKey(KeyCode.A))
        {
            
            transform.Translate(new Vector2(-speed, 0));

            if (!izq)
            {
                transform.position = new Vector3(gameObject.transform.position.x - 2.29f, gameObject.transform.position.y, gameObject.transform.position.z);
                izq = true;
            }
            
            der = false;
        }

        


    }
}
