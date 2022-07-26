using UnityEngine;
using  DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private bool followOnX = false;
    
    // private variables
    private Transform target = default;

    private Vector3 velocity = Vector3.zero;
    private Vector3 offSet = Vector3.zero;
    private Vector3 targetPosition;

    private void Start()
    {
        target = GameReferenceHolder.Instance.playerController.transform;
        offSet = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if(!target) return;

        targetPosition = target.position + offSet;
        targetPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        if (!followOnX)
        {
            targetPosition.x = transform.position.x;
        }

        transform.position = targetPosition;
    }

    public void Shake(float duration)
    {
        transform.DOShakeRotation(duration, 2, 1);
    }
}
