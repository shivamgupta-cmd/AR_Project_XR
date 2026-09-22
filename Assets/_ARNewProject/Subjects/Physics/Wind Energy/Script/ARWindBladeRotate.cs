using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARWindBladeRotate : MonoBehaviour
{
    [Header("Blade")]
    [SerializeField] private Transform blade;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 80f;

    private void Update()
    {
        if (blade == null)
            return;

        blade.Rotate(0f,0f,rotationSpeed * Time.deltaTime,Space.Self);
    }
}
