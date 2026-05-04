using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public EnemyBase[] enemy;

    private Wave currentWave;
    private int currentWaveNumber = 0;
    
    private int enemiesRemainingTospawn;
    private int enemiesRemainingAlive;
    private float nextSpawnTime;


    private MapGenerator map;

    private LivingEntity playerEntity;
    private Transform playerT;
    
    float timeBetweenCampingChecks = 2;
    float campThresholdDistance = 1.5f;
    float nextCampingCheckTime;
    Vector3 campPositionOld;
    private bool isCamping;
    private bool isDisabling;
    
    public event System.Action<int> OnNewWave;

    public bool devMode;
    
    

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;
        nextCampingCheckTime = Time.time + timeBetweenCampingChecks;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;
        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void Update()
    {
        if (!isDisabling)
        {
            if (Time.time > nextCampingCheckTime)
            {
                nextCampingCheckTime = Time.time + timeBetweenCampingChecks;
                isCamping = Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance;
                campPositionOld = playerT.position; 
            }
        
            if ((enemiesRemainingTospawn > 0 || currentWave.infinite) && Time.time >= nextSpawnTime)
            {
                enemiesRemainingTospawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                // EnemyBase spawnedEnemy = Instantiate(enemy[Random.Range(0,2)], Vector3.zero, Quaternion.identity) as EnemyBase;
                // spawnedEnemy.OnDeath += OnEnemyDeath;
                StartCoroutine(SpawnEnemy());
            }
        }

        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //StopCoroutine("SpawnEnemy");
                StopAllCoroutines();
                foreach (EnemyBase enemy in FindObjectsOfType<EnemyBase>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }

    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;
        
        
        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }
        Material tilemat = spawnTile.GetComponent<Renderer>().material;
        Color initialcolor = Color.white;
        Color flashcolor = Color.red;
        float spawnTimer = 0;
        
        while (spawnTimer < spawnDelay)
        {
            tilemat.color = Color.Lerp(initialcolor, flashcolor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
            
            
        }
        
        EnemyBase spawnedEnemy = Instantiate(enemy[Random.Range(0,2)], spawnTile.position + Vector3.up , Quaternion.identity) as EnemyBase;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColour);
    }
    

    
    void OnPlayerDeath()
    {
        isDisabling = true;
    }
    
    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    void OnEnemyDeath()
    {
        //Debug.Log("Enemy Died");

        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0)
        {
            NextWave();
        }



    }

    void NextWave()
    {
        if (currentWaveNumber > 0) AudioManager.instance.PlaySound2D ("Level Complete");
        currentWaveNumber++;
        print("wave: " + currentWaveNumber);
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingTospawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingTospawn;
            
            if (OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();
        }

        
        

    }
    
    
    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColour;

    }
}
