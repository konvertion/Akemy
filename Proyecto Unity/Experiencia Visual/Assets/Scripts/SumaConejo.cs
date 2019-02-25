using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumaConejo : MonoBehaviour
{
    [SerializeField] GameObject Conejo;
    [SerializeField] GameObject Akemy;
    [SerializeField] int posicion;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Akemy)
        {
          //  Debug.Log("colision confirmada perro");
            Conejo.GetComponent<ConejoControl>().WaypointIndex = posicion;

        }
    }

}
