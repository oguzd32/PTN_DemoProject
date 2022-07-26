using System;
using UnityEngine;
using  DG.Tweening;
using static Utilities;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;
    
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

    public void FailProcess()
    {
        characterAnimator.SetBool("Fail", true);
        movement.enabled = false;
        transform.DOMoveZ(transform.position.z - 1, 1f);
        _GameManager.EndGame(false);
    }
}
