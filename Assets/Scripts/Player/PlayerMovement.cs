using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float horizontalSpeed = 3f;
    [SerializeField] private float clampX = 1.7f;
    [SerializeField] private bool isClamp = false;

    public bool isFail { get; set; } = false;
    
    // cached components
    private CharacterController cc;

    // private variables
    private bool isStarted = false;
    private float _ForwardSpeed;
    private float maxSpeed;

    private Vector3 speed;
    
    void Start()
    {
        cc = GetComponent<CharacterController>();
        maxSpeed = forwardSpeed;
    }

    private void OnDisable()
    {
        _ForwardSpeed = 0;
    }

    internal void StartGame()
    {
        isStarted = true;
    }
    
    void Update()
    {
        if(!isStarted || isFail) return;
        
        speed.x = TouchInput.SwerveDeltaX * 0.05f * horizontalSpeed;
        _ForwardSpeed = Mathf.Clamp(_ForwardSpeed + 1, 0, maxSpeed);
        speed.z = _ForwardSpeed;
        cc.SimpleMove(speed);
        
        if(isClamp){Clamp();}
    }

    private void Clamp()
    {
        Vector3 tempPos = transform.position;

        tempPos.x = Mathf.Clamp(tempPos.x, -clampX, clampX);

        transform.position = tempPos;
    }
}
