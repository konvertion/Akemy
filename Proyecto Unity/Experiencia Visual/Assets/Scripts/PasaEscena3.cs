﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasaEscena3 : MonoBehaviour
{
    void Start()
    {

    }
    void OnBecameInvisible()
    {
        SceneManager.LoadScene("Final");
    }
}
