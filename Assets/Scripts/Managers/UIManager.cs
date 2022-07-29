using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public UnityEvent OnLevelStarted;

    [Header("GameObject References")] 
    [SerializeField] private GameObject startUI = default;
    [SerializeField] private GameObject inGameUI = default;
    [SerializeField] private GameObject winUI = default;
    [SerializeField] private GameObject failUI = default;

    [Header("Text References")] 
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI inGameLevelText;
    [SerializeField] private TextMeshProUGUI winGameLevelText;    
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Rectransform References")]
    [SerializeField] private RectTransform coinIconPrefab;
    [SerializeField] private RectTransform coinIcon;

    // private variables
    private int currentLevel = 0;
    private int currentCoinCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentLevel = PlayerPrefs.GetInt("Level", 0);
        currentCoinCount = PlayerPrefs.GetInt("Coin", 0);
        levelText.text = inGameLevelText.text = $"LEVEL {currentLevel + 1}";
        winGameLevelText.text = $"LEVEL {currentLevel + 1} COMPLETED";
        coinText.text = currentCoinCount.ToString();
    }

    internal IEnumerator SpawnCoin(Vector3 worldPos, int amount)
    {
        Sequence sequence = DOTween.Sequence();
        
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForEndOfFrame();
            SpawnCoin(worldPos);
        }
    }

    internal void SpawnCoin(Vector3 worldPos)
    {
        Vector3 screenPos = Vector3.zero;

        if (worldPos != Vector3.zero)
        {
            screenPos = Camera.main.WorldToScreenPoint(worldPos);
        }
        else
        {
            screenPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        }

        RectTransform current = Instantiate(coinIconPrefab, screenPos, Quaternion.identity, transform);
        current.DOPunchScale(Vector3.one * .5f, .2f, 5);
        current.DOMove(coinIcon.position, 0.9f).OnComplete(() =>
            {
                Destroy((current.gameObject));
                currentCoinCount += 10;
                coinText.text = currentCoinCount.ToString();
            }
        );
    }

    #region Main Functions
    
    public void OnPlayerStartedLevel()
    {
        startUI.SetActive(false);
        inGameUI.SetActive(true);
        OnLevelStarted?.Invoke();
    }

    public void OnPlayerCompletedLevel()
    {
        inGameUI.SetActive(false);
        winUI.SetActive(true);
    }

    public void OnPlayerFailedLevel()
    {
        inGameUI.SetActive(false);
        coinIcon.transform.parent.gameObject.SetActive(false);
        failUI.SetActive(true);
    }
    
    #endregion
    
    #region Button Functions
    
    public void OnPlayButtonClicked()
    {
        OnPlayerStartedLevel();
    }

    public void OnRetryButtonClicked()
    {
        DOTween.KillAll();
        PlayerPrefs.SetInt("Coin", currentCoinCount);
        LevelLoader.Instance.RestartLevel();
    }

    public void OnNextButtonClicked()
    {
        DOTween.KillAll();
        PlayerPrefs.SetInt("Coin", currentCoinCount);
        LevelLoader.Instance.NextLevel();
    }
    
    #endregion
}
