using UnityEngine;

public static class Utilities
{
    public static ObjectPooler _ObjectPooler => ObjectPooler.Instance;
    public static GameReferenceHolder _GameReferenceHolder => GameReferenceHolder.Instance;
    public static GameManager _GameManager => GameManager.Instance;
}

public class GameReferenceHolder : MonoBehaviour
{
    public static GameReferenceHolder Instance;

    [Header("GameObject References")] 
    public GameObject playerGameObject;
    public GameObject[] finishConfetties;

    [Header("Script References")] 
    public PlayerController playerController;
    public CameraFollow cameraFollow;
    
    private void Awake()
    {
        Instance = this;
    }
}
