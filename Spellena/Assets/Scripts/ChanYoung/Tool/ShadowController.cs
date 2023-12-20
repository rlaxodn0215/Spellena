using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    void Update()
    {
        Ray _ray = new Ray();
        _ray.origin = transform.root.position;
        _ray.direction = -transform.up;
        RaycastHit _rayHit;
        LayerMask layerMask = LayerMask.GetMask("Map");
        if (Physics.Raycast(_ray, out _rayHit, Mathf.Infinity, layerMask))
        {
            transform.position = _rayHit.point + new Vector3(0, 0.05f, 0);
            Debug.Log("üũ");
        }
    }
}
