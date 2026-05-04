using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorekeeper : MonoBehaviour
{
    public static int score { get; private set; }
    float lastEnemyKilledTime;
    int streakCount;
    private int streakExpiryTime = 1;
    void Start()
    {
        EnemyBase.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;

    }
    
    void OnEnemyKilled()
    {
        if (Time.time > lastEnemyKilledTime + streakExpiryTime)
        {
            streakCount++;
        }
        else streakCount = 0;

        lastEnemyKilledTime = Time.time;

        score += 5 + (int)Mathf.Pow(2,streakCount);
    } 
    
    void OnPlayerDeath()
    {
        EnemyBase.OnDeathStatic -= OnEnemyKilled;
    }
}
