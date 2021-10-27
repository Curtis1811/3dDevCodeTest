using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    public static Manager instance = null;


    private Camera cam;
    public GameObject ObjectToCenterAround;
    private Vector3 offset = new Vector3(0,1.55f,-1.70f);

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void resetCameraOrientation()
    {
        //StartCoroutine("AnimateCamera");
    }

    
    public void getObjectReferance()
    {

    }

}
