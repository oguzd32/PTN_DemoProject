using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RankingController : MonoBehaviour
{
    public static RankingController instance;
    
    [SerializeField] private TextMeshProUGUI[] rankTexts;

    private List<Transform> topSix;
    private List<Transform> players;

    public bool isFinish { get; set; } = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        players = new List<Transform>();

        topSix = new List<Transform>(6);
        
        players.Add(GameReferenceHolder.Instance.playerController.transform);

        foreach (EnemyController enemy in GameReferenceHolder.Instance.enemies)
        {
            players.Add(enemy.transform);
        }

        for (int i = 0; i < rankTexts.Length; i++)
        {
            topSix.Add(players[i]);
        }
    }

    private void LateUpdate()
    {
        if(isFinish) return;
        
        players = players.OrderByDescending(player => player.position.z).ToList();

        for (int i = 0; i < topSix.Count; i++)
        {
            topSix[i] = players[i];
            rankTexts[i].text = $"{i + 1} {topSix[i].name}";
        }
    }
}
