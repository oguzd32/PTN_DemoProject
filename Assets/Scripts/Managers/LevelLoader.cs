using System;
using DG.Tweening;
using UnityEngine;
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    public static event Action<int> OnLevelChanged;
    [SerializeField] private GameObject[] levels;
    
    // private variables
    private int level = 0;
    private GameObject currentLevel = null;
    
    private void Awake()
    {
        Instance = this;
        LoadLevel(ClampLevel(level));
    }

    public void NextLevel()
    {
        level++;
        PlayerPrefs.SetInt("Level", level);
        LoadLevel(ClampLevel(level));
    }

    public void RestartLevel()
    {
        LoadLevel(ClampLevel(level));
    }

    private void LoadLevel(int level)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }
        currentLevel = Instantiate(levels[level], Vector3.zero, Quaternion.identity);
        OnLevelChanged?.Invoke(level);
    }
    private int ClampLevel(int level)
    {
        return level % levels.Length;
    }
}