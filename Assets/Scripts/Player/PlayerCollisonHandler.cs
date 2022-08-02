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

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(controller.isFail) return;
        
        GameObject otherObj = other.gameObject;

        switch (otherObj.tag)
        {
            case "Obstacle":

                HitObstacleProcess(otherObj);
                break;
            
            case "Finish":

                FinishProcess();
                break;
            
            case "RotatorStick":

                HitRotatorStickProcess();
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(controller.isFail) return;

        OnRotatorProcess(other.gameObject);
    }

    private void OnRotatorProcess(GameObject rotatorObj)
    {
        if (!rotatorObj.TryGetComponent(out Rotator rotator)) return;
        
        transform.position += Vector3.right * (rotator.GetDirection * Time.deltaTime);

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

    private void HitRotatorStickProcess()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => movement.enabled = false);
        sequence.Append(transform.DOMoveZ(transform.position.z - 10f, 1f));
        sequence.AppendCallback(() => movement.enabled = true);
    }
    
    private void HitObstacleProcess(GameObject hitObj)
    {
        controller.isFail = true;
                
        GameObject obj = _ObjectPooler.GetPooledObject("Hit");
        obj.transform.position = hitObj.transform.position;
        obj.SetActive(true);
        transform.DOMoveZ(transform.position.z - 2, 1f);
        controller.FailProcess();
    }
    
    private void FinishProcess()
    {
        RankingController.instance.isFinish = true;
        Input.ResetInputAxes();
        _GameReferenceHolder.cameraFollow.SetTarget(_GameReferenceHolder.paintManager.transform, new Vector3(0, 5, -8.5f));
        movement.enabled = false;
        controller.SetAnimation("Move", false);
        transform.DOMoveX(0, 1f);
        controller.inFinal = true;
        _GameReferenceHolder.paintManager.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(8f);
        sequence.AppendCallback(() => controller.SetAnimation("Win", true));
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
    }
}
