using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class EnemyMovement : MonoBehaviour
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

    private Vector3 speed;
    
    void Start()
    {
        cc = GetComponent<CharacterController>();
        _ForwardSpeed = forwardSpeed;
    }

    internal void StartGame()
    {
        isStarted = true;
    }
    
    void Update()
    {
        if(!isStarted) return;
        
        speed.z = _ForwardSpeed;
        cc.SimpleMove(speed);
        
        if(isClamp){Clamp();}
    }

    public void Swerve(bool isLeft)
    {
        transform.DOKill();

        float speed;
        Sequence sequence = DOTween.Sequence();

        if (isLeft) { speed = 4f; }
        else { speed = -4f; }

        sequence.AppendCallback(() => this.speed.x = speed);
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() => this.speed.x = 0);
    }
    
    private void Clamp()
    {
        if(!isStarted) return;
        
        Vector3 tempPos = transform.position;

        tempPos.x = Mathf.Clamp(tempPos.x, -clampX, clampX);

        transform.position = tempPos;
    }
}