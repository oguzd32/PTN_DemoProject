using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector3 direction;

    public float GetDirection => -direction.z * speed * 0.15f;

    private void Update()
    {
        transform.Rotate(direction * speed * Time.deltaTime);
    }
}
