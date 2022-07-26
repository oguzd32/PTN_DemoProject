using UnityEngine;

public class TouchInput : MonoBehaviour
{
    [Header("Swerve Settings")]
    [SerializeField] private float swerveThreshold = 2f;

    private static float _swerveLastXPos = 0;
    private static float _swerveLastYPos = 0;
    private static float _swerveDeltaX = 0;
    private static float _swerveDeltaY = 0;

    public static float SwerveDeltaX => _swerveDeltaX;
    public static float SwerveDeltaY => _swerveDeltaY;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _swerveLastXPos = Input.mousePosition.x;
            _swerveLastYPos = Input.mousePosition.y;
        }
        else if (Input.GetMouseButton(0))
        {
            _swerveDeltaX = Input.mousePosition.x - _swerveLastXPos;
            _swerveDeltaY = Input.mousePosition.y - _swerveLastYPos;

            _swerveLastXPos = Input.mousePosition.x;
            _swerveLastYPos = Input.mousePosition.y;

            if (Mathf.Abs(_swerveDeltaX) <= swerveThreshold) _swerveDeltaX = 0f;
            if (Mathf.Abs(_swerveDeltaY) <= swerveThreshold) _swerveDeltaY = 0f;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _swerveDeltaX = _swerveDeltaY = 0;
        }
    }
}