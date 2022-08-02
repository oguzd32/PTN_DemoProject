using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float clampX = 1.7f;
    [SerializeField] private bool isClamp = false;
    [SerializeField] private LayerMask obstacleLayer;
    
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
        if(!isStarted) return;

        _ForwardSpeed = Mathf.Clamp(_ForwardSpeed + 1, 0, maxSpeed);
        speed.z = _ForwardSpeed;
        cc.SimpleMove(speed);
        
        CheckForward();
        
        if(isClamp) Clamp();
    }

    private void CheckForward()
    {
        RaycastHit hit;
        
        if(!Physics.Raycast(transform.position + Vector3.up, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, obstacleLayer)) return;

        Debug.DrawRay(transform.position + Vector3.up * .5f, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
        
        Debug.Log("did hit");
        
        if (hit.transform.position.z - transform.position.z < 10)
        {
            if (transform.position.x > 0)
            {
                Debug.Log("run");
                isRun = true;
                //Swerve(true);
                transform.DOMoveX(transform.position.x - 2, 1f);
            }
            else
            {
                Debug.Log("run");
                isRun = true;
                //Swerve(false);
                transform.DOMoveX(transform.position.x + 2, 1f);
            }
        }
    }

    private bool isRun = false;
    public void Swerve(bool isLeft)
    {
        if(isRun) return;
        
        transform.DOKill();

        float speed;
        Sequence sequence = DOTween.Sequence();

        if (isLeft) { speed = 2f; }
        else { speed = -2f; }

        sequence.AppendCallback(() => this.speed.x = speed);
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() => this.speed.x = 0);
        sequence.OnComplete(() => isRun = false);
    }
    
    private void Clamp()
    {
        if(!isStarted) return;
        
        Vector3 tempPos = transform.position;

        tempPos.x = Mathf.Clamp(tempPos.x, -clampX, clampX);

        transform.position = tempPos;
    }
}