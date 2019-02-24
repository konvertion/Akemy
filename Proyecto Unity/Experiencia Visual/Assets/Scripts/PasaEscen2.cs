using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasaEscen2 : MonoBehaviour
{
    void Start()
    {

    }
    void OnBecameInvisible()
    {
        SceneManager.LoadScene("Acto 2");
    }
}
