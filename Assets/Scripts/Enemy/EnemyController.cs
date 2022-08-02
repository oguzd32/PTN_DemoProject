using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private float swerveInterval = 5f;
    
    public bool isFail { get; set; } = false;
    public bool inFinal { get; set; } = false;
    
    // cached components
    private EnemyMovement movement;

    // private variables
    private double lastSwerveTime = 0;
    private bool isStarted = false;

    private void Start()
    {
        isFail = false;
        movement = GetComponent<EnemyMovement>();
    }
    
    internal void StartGame()
    {
        characterAnimator.SetBool("Move", true);
        isStarted = true;
        movement.StartGame();
    }

    private void Update()
    {
        if(isFail || !isStarted) return;
        
        if (Mathf.Abs(transform.position.x) > 5f)
        {
            isFail = true;
            transform.DOMoveY(transform.position.y - 10, 3f);
            FailProcess();
        }
        
        RandomSwerve();
    }

    private void RandomSwerve()
    {
        float randomTime = Random.Range(0f, 3f);
        
        if(Time.time < lastSwerveTime + swerveInterval + randomTime) return;

        lastSwerveTime = Time.time;

        float randomNumb = Random.Range(0f, 1f);
        
        //movement.Swerve(randomNumb > 0.5f);
    }
    
    public void SetAnimation(string stateName, bool value) => characterAnimator.SetBool(stateName, value);
    
    public void FailProcess()
    {
        characterAnimator.SetBool("Fail", true);
        movement.enabled = false;
    }
}
