using System.Collections;
using DG.Tweening;
using UnityEngine;
using static Utilities;

public class EnemyCollisonHandler : MonoBehaviour
{
    // cached components
    private EnemyMovement movement;
    private EnemyController controller;

    // private variables
    private GameObject lastTriggeredObj = default;

    private void Start()
    {
        controller = GetComponent<EnemyController>();
        movement = GetComponent<EnemyMovement>();
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

                HitObstacleProcess(otherObj);
                break;
            
            case "RotatorStick":
                HitRotatorStickProcess();
                break;

            case "Finish":

                FinishProcess();
                break;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag.Equals("Player"))
        {
            movement.transform.DOKill();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(controller.isFail) return;

        OnRotatorProcess(other.gameObject);
    }

    private void OnRotatorProcess(GameObject otherObj)
    {
        if (!otherObj.TryGetComponent(out Rotator rotator)) return;

        transform.position += Vector3.right * (rotator.GetDirection * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > 2)
        {
            controller.isFail = true;

            Sequence sequence = DOTween.Sequence();

            if (transform.position.x > 0)
            {
                sequence.Append(transform.DOMoveX(transform.position.x + 2, 1f));
            }
            else
            {
                sequence.Append(transform.DOMoveX(transform.position.x - 2, 1f));
            }

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
    
    private void FinishProcess()
    {
        movement.enabled = false;

        float randomX = 0;

        while (Mathf.Abs(randomX) < 1.5f)
        {
            randomX = Random.Range(-3f, 3f);
        }

        transform.DOMoveX(randomX, .5f);
        controller.SetAnimation("Move", false);
        controller.inFinal = true;
        controller.SetAnimation("Win", true);
    }
    
    private void HitObstacleProcess(GameObject otherObj)
    {
        controller.isFail = true;

        GameObject obj = _ObjectPooler.GetPooledObject("Hit");
        obj.transform.position = otherObj.transform.position;
        obj.SetActive(true);
        transform.DOMoveZ(transform.position.z - 2, 1f);
        controller.FailProcess();
    }
}
