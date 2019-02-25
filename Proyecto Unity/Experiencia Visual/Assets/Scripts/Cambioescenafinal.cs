using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cambioescenafinal : MonoBehaviour
{
    [SerializeField] GameObject Akemy;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Akemy)
        {
            SceneManager.LoadScene("Final");
        }
    }
}
