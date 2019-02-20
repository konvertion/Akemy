using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caminata : MonoBehaviour
{
    
    public float speed = 2f;

    
    

    // Update is called once per frame
    void Update()
    {
       
       if(Input.GetKey(KeyCode.D))
        {

            if(GetComponent<SpriteRenderer>().flipX)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            transform.Translate(new Vector2(speed, 0));
            GetComponent<Animator>().SetBool("Correr", true);
           
        }
       
        if (Input.GetKey(KeyCode.A))
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            transform.Translate(new Vector2(-speed, 0));
            GetComponent<Animator>().SetBool("Correr", true);
        }

        if (Input.GetKeyUp(KeyCode.D) || (Input.GetKeyUp(KeyCode.A)))
        {
            GetComponent<Animator>().SetBool("Correr", false);
        }


    }
}
