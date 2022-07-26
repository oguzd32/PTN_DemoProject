using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float horizontalSpeed = 3f;
    [SerializeField] private float clampX = 1.7f;

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

    // Update is called once per frame
    void Update()
    {
        if(!isStarted) return;
        
        speed.x = TouchInput.SwerveDeltaX * 0.05f * horizontalSpeed;
        speed.z = _ForwardSpeed;
        cc.SimpleMove(speed);
        
        Clamp();
    }

    private void Clamp()
    {
        Vector3 tempPos = transform.position;

        tempPos.x = Mathf.Clamp(tempPos.x, -clampX, clampX);

        transform.position = tempPos;
    }
}
