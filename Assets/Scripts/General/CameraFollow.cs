using UnityEngine;
using  DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private bool followOnX = false;
    [SerializeField] private Vector3 inGameOffSet;
    
    // private variables
    private Transform target = default;

    private Vector3 velocity = Vector3.zero;
    [SerializeField] private Vector3 offSet = Vector3.zero;
    private Vector3 targetPosition;

    internal void StartGame()
    {
        offSet = inGameOffSet;
    }
    
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

    public void SetTarget(Transform target, Vector3 offSet)
    {
        this.target = target;
        this.offSet = offSet;
    }
}
