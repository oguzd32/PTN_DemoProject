using System;
using UnityEngine;
using  DG.Tweening;
using Unity.Mathematics;
using XDPaint;
using static Utilities;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;

    public bool isFail { get; set; } = false;
    public bool inFinal { get; set; } = false;
    
    // cached components
    private PlayerMovement movement;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    internal void StartGame()
    {
        characterAnimator.SetBool("Move", true);
        movement.StartGame();
    }

    private void Update()
    {
        if(isFail) return;
        
        if (math.abs(transform.position.x) > 5f)
        {
            isFail = true;
            transform.DOMoveY(transform.position.y - 10, 3f);
            FailProcess();
        }
    }

    public void SetAnimation(string stateName, bool value) => characterAnimator.SetBool(stateName, value);

    public void FailProcess()
    {
        _GameReferenceHolder.cameraFollow.SetTarget(null, Vector3.zero);
        characterAnimator.SetBool("Fail", true);
        movement.enabled = false;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => _GameManager.EndGame(false));
    }
}
