using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public RectTransform newWaveBanner;
    public TMP_Text newWaveTitle;
    public TMP_Text newWaveEnemyCount;
    public TMP_Text scoreUI;
    public TMP_Text gameOverScoreUI;
    public RectTransform healthBar;
    

    public GameObject gameOverUI;

    private Spawner spawner;

    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }
    
    void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    private void Update()
    {
        scoreUI.text = Scorekeeper.score.ToString("D6");
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
    }

    void OnNewWave(int waveNumber)
    {
        
        string[] numbers = {"One","Two","Three","Four","Five"};
        newWaveTitle.text = "- Wave " + numbers[waveNumber-1] + " -";
        string enemyCountString = spawner.waves[waveNumber-1].infinite ? "Infinite" : spawner.waves[waveNumber-1].enemyCount.ToString();
        newWaveEnemyCount.text = "Enemies " + enemyCountString;
        
        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float animatepercent = 0;
        float speed = 2.5f;
        float delayTime = 1.5f;
        int dir = 1;
        float endDelayTime = Time.time + 1/speed + delayTime;

        while (animatepercent >= 0)
        {
            animatepercent += Time.deltaTime * speed* dir;
            if (animatepercent >= 1)
            {
                animatepercent = 1;
                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }
            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-150, 50, animatepercent);
            yield return null;
        }
    }
    void OnGameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear,new Color(0,0,0,.95f),1));
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
        
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }
    
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
