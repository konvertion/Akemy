﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoEvento : MonoBehaviour
{
    [SerializeField] VideoClip video;
    [SerializeField] PasaEscen2 pasaescena2;
    [SerializeField] float duracion;
    float time;
    private AudioSource musicPlayer;


    // Start is called before the first frame update
    void Start()
    {
        musicPlayer = Camera.main.gameObject.GetComponent<AudioSource>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log ("atraveso colider");
        if (!Camera.main.gameObject.GetComponent<VideoPlayer>().isPlaying)
        {
            Camera.main.gameObject.GetComponent<VideoPlayer>().clip=video;

            Camera.main.gameObject.GetComponent<VideoPlayer>().Play();

            musicPlayer.Stop();
        }
        
    }

   
    
    // Update is called once per frame
    void Update()
    {

        if (Camera.main.gameObject.GetComponent<VideoPlayer>().isPlaying)
            time += Time.deltaTime;
        if(time>=duracion)
            pasaescena2.CambiarEscena();
    }
}
