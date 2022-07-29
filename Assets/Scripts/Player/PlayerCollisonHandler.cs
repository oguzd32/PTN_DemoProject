using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using static Utilities;

public class PlayerCollisonHandler : MonoBehaviour
{
    // cached components
    private PlayerController controller;
    private PlayerMovement movement;

    // private variables
    private GameObject lastTriggeredObj = default;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(controller.isFail) return;
        
        GameObject otherObj = other.gameObject;

        // check same object multiple trigger
        if(lastTriggeredObj == otherObj) return;
        lastTriggeredObj = otherObj;
        
        switch (otherObj.tag)
        {
            case "Obstacle":

                controller.isFail = true;
                
                GameObject obj = _ObjectPooler.GetPooledObject("Hit");
                obj.transform.position = otherObj.transform.position;
                obj.SetActive(true);
                transform.DOMoveZ(transform.position.z - 2, 1f);
                controller.FailProcess();
                break;
            
            case "Finish":

                movement.enabled = false;
                controller.SetAnimation("Move", false);
                transform.DOMoveX(0, .5f);
                controller.inFinal = true;
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(8f);
                sequence.AppendCallback(() => _GameReferenceHolder.paintManager.enabled = false);
                sequence.AppendCallback(() => StartCoroutine(UIManager.Instance.SpawnCoin(transform.position, 10)));
                sequence.AppendInterval(1f);
                sequence.AppendCallback(() => GameManager.Instance.EndGame(true));
                sequence.AppendCallback(() =>
                {
                    foreach (GameObject confetti in _GameReferenceHolder.finishConfetties)
                    {
                        confetti.SetActive(true);
                    }
                });
                break;
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(controller.isFail) return;
        
        GameObject otherObj = other.gameObject;
        
        if (otherObj.TryGetComponent(out Rotator rotator))
        {
            transform.position += Vector3.right * (rotator.GetDirection * Time.deltaTime);
        }

        if (Mathf.Abs(transform.position.x) > 2)
        {
            controller.isFail = true;

            Sequence sequence = DOTween.Sequence();
            
            if (transform.position.x > 0) { sequence.Append(transform.DOMoveX(transform.position.x + 2, 1f));}
            else{sequence.Append(transform.DOMoveX(transform.position.x - 2, 1f));}

            sequence.Join(transform.DOMoveY(transform.position.y - 10, 3f));

            controller.FailProcess();
        }
    }
}
