using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class pasaescena : MonoBehaviour
{
    void Start()
    {
        
    }
    void OnBecameInvisible()
    {    
      SceneManager.LoadScene("acto 1");       
    }
}
