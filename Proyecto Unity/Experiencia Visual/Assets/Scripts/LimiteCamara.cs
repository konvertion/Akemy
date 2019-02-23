using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimiteCamara : MonoBehaviour
{
    public Transform target;
    //Se crear variables para poder habilitar y colocar el valor maximo y minimo donde llegara la camara
    public bool YMaxEnabled = false;
    public float YMaxValue = 0;
    public bool YMinEnabled = false;
    public float YMinValue = 0;
    public bool XMaxEnabled = false;
    public float XMaxValue = 0;
    public bool XMinEnabled = false;
    public float XMinValue = 0;

    void FixedUpdate()
    {
        //posicion del target
        Vector3 targetpos = target.position;

        //Vertical
        if (YMinEnabled && YMaxEnabled)
        { targetpos.y = Mathf.Clamp(target.position.y, YMinValue, YMaxValue); }
        else if (YMinEnabled)
        { targetpos.y = Mathf.Clamp(target.position.y, YMinValue, target.position.y); }
        else if (YMaxEnabled)
        { targetpos.y = Mathf.Clamp(target.position.y, target.position.y, YMaxValue); }
        //Horizontal
        if (XMinEnabled && XMaxEnabled)
        { targetpos.x = Mathf.Clamp(target.position.x, XMinValue, XMaxValue); }
        else if (XMinEnabled)
        { targetpos.x = Mathf.Clamp(target.position.x, XMinValue, target.position.x); }
        else if (XMaxEnabled)
        { targetpos.x = Mathf.Clamp(target.position.x, target.position.x, XMaxValue); }
    }
}
