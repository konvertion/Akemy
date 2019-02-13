using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{

    private ParticleSystem ps;
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (ps.isEmitting)
            {
                ps.Stop();
                Debug.Log("denetiendo particulas");

            }
            else
            {
                ps.Play();
            }
        }

    }
}

