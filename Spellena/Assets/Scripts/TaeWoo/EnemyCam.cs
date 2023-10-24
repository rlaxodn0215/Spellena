using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCam : MonoBehaviour
{
    private Camera camera;
    private Transform parent;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        camera = GetComponent<Camera>();
        parent = transform.root;

        if(parent.CompareTag("TeamA"))
        {
            camera.cullingMask |= (1 << LayerMask.NameToLayer("TeamB"))|(1 << LayerMask.NameToLayer("SpawnObjectB"));
        }

        else if(parent.CompareTag("TeamB"))
        {
            camera.cullingMask |= 1 << LayerMask.NameToLayer("TeamA") | (1 << LayerMask.NameToLayer("SpawnObjectA"));
        }
    }


}
