using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject[] Cameras;

    private void Start()
    {
        setActive(0);
    }

    public void setActive(int cam)
    {
        for (int i = 0; i < Cameras.Length; i++)
        {
            if(i == cam)
            {
                Cameras[i].SetActive(true);
            } else
            {
                Cameras[i].SetActive(false);
            }
        }
    }
}
