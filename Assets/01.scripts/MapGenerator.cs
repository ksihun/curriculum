using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;
    
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    [FormerlySerializedAs("naveMeshFloor")] public Transform navmeshFloor;
    public Transform mapFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;
    
    int[] dx = new int[] {0,1,0,-1};
    int[] dy = new int[] {1,0,-1,0};

    [Range(0, 1)] public float outlinePercent;
    
    public float tileSize;


    private List<Coord> allTileCoords;
    
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;

    private Transform[,] tilemap;
    
    Map currentMap;
    
    
    
    public void Start()
    {
        FindObjectOfType<Spawner> (). OnNewWave += OnNewWave;
    }

    private void OnNewWave(int waveNumber)
    {
        Debug.Log ("OnNewWave " + waveNumber);
        mapIndex = waveNumber - 1;
        GeneratorMap();

    }

    public void GeneratorMap()
    {
        currentMap = maps[mapIndex];
        tilemap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        //GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize,.05f, currentMap.mapSize.y * tileSize);
        
        //좌표
        allTileCoords = new List<Coord>();

        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(),currentMap.seed));
        

        //맵 홀더 생성
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
            
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        
        //타일 생성
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1- outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tilemap[x, y] = newTile;
            }
        }

        //장애물 생성
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);
        
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;
            if (randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleheight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition+Vector3.up*obstacleheight/2f, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1- outlinePercent) * tileSize, obstacleheight, (1- outlinePercent) * tileSize);
                
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                
                float colorPercent = (float)randomCoord.y / currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                
                
                obstacleRenderer.sharedMaterial = obstacleMaterial;
                
                allOpenCoords.Remove(randomCoord);
                


            }
            else{
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
            
        }
        
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(),currentMap.seed));
        
        
        //이동 경로 제한
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x+maxMapSize.x)/4f *tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x)/2f, 1 , currentMap.mapSize.y)* tileSize;
        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x+maxMapSize.x)/4f *tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x)/2f, 1 , currentMap.mapSize.y)* tileSize;
        
        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y+maxMapSize.y)/4f *tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;
        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y+maxMapSize.y)/4f *tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;

        
        
        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y)*tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);

    }


    //BFS
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;
        
        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int i = 0; i < 4f; i++)
            {
                int neighborX = tile.x + dx[i];
                int neighborY = tile.y + dy[i];
                if (neighborX >= 0 && neighborX < obstacleMap.GetLength(0) && neighborY >= 0 && neighborY < obstacleMap.GetLength(1))
                {
                    if (!mapFlags[neighborX, neighborY] && !obstacleMap[neighborX, neighborY])
                    {
                        mapFlags[neighborX, neighborY] = true;
                        queue.Enqueue(new Coord(neighborX, neighborY));
                        accessibleTileCount++;
                    }
                }
            }
            
        }
        
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;

    }

    



    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }
    
    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tilemap[randomCoord.x, randomCoord.y];
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + .5f + x, 0, -currentMap.mapSize.y / 2f + .5f + y) * tileSize;
    }
    
    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize  + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize  + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp(x, 0, tilemap.GetLength(0)-1);
        y = Mathf.Clamp(y, 0, tilemap.GetLength(1)-1);
        
        return tilemap[x, y];
    }
    
    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
        
        
        
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0, 1)] public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;


        public Coord mapCentre {
            get{
                return new Coord(mapSize.x/2, mapSize.y/2);
            }
        }   
    }
    
    

    
    
}
