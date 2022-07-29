using UnityEngine;
using XDPaint.Controllers;
using static Utilities;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // private variables
    private bool isFinished = false;
    
    private void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        _GameReferenceHolder.playerController.StartGame();
        _GameReferenceHolder.cameraFollow.StartGame();
        foreach (EnemyController enemy in _GameReferenceHolder.enemies)
        {
            enemy.StartGame();
        }
        _GameReferenceHolder.paintManager.gameObject.SetActive(true);
    }

    public void EndGame(bool win, int amount = 0)
    {
        if (isFinished) return;

        isFinished = true;
        
        if (win)
        {
            UIManager.Instance.OnPlayerCompletedLevel();
            StartCoroutine(UIManager.Instance.SpawnCoin(_GameReferenceHolder.playerGameObject.transform.position,
                amount));
        }
        else
        {
            UIManager.Instance.OnPlayerFailedLevel();
        }
    }
}
