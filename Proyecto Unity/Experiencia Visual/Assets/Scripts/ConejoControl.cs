using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConejoControl : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;
    [SerializeField] float movespeed = 2f;
    int waypointIndex = 0;

    public int WaypointIndex { get => waypointIndex; set => waypointIndex = value; }


    // Start is called before the first frame update
    void Start()
    {
        transform.position = waypoints[WaypointIndex].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {

        transform.position = Vector2.MoveTowards(transform.position,
            waypoints[WaypointIndex].transform.position, movespeed
            * Time.deltaTime);
        GetComponent<Animator>().SetBool("Andar", true);

        if (Vector2.Distance(transform.position, waypoints[WaypointIndex].transform.position) <= 0.1f)
        {
            
            
            GetComponent<Animator>().SetBool("Andar", false);
        }
        if (WaypointIndex == waypoints.Length)
        {
            WaypointIndex = 0 ;
        }
    }
}
