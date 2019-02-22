using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caminata : MonoBehaviour
{
    
    public float speed = 2f;
    Vector3 scale;

    private void Awake()
    {
        scale = transform.localScale;
    }



    // Update is called once per frame
    void Update()
    {
       
       if(Input.GetKey(KeyCode.D))
        {
            transform.localScale = scale;
            if(GetComponent<SpriteRenderer>().flipX)
            {
                //GetComponent<SpriteRenderer>().flipX = false;
                
            }
            transform.Translate(new Vector2(speed, 0));
            GetComponent<Animator>().SetBool("Correr", true);
           
        }
       
        if (Input.GetKey(KeyCode.A))
        {
            transform.localScale = new Vector3(-scale.x,scale.y,scale.z);
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                //GetComponent<SpriteRenderer>().flipX = true;
                
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
