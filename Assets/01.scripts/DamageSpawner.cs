using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamageSpawner : MonoBehaviour
{
    public static DamageSpawner Instance { get; private set; }
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private WorldDamageText worldDamageTextPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private bool useWorldDamageText = true;
    private Camera cam;
    
    private ObjectPool<WorldDamageText> worldDamagePool;
    private ObjectPool<DamageText> uiDamagePool; 
    //유니티 내장 오브젝트 풀링
    
    [SerializeField] private Transform worldPoolParent;
    [SerializeField] private Transform uiPoolParent;
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cam = Camera.main;
        
        worldDamagePool = new ObjectPool<WorldDamageText>(
            CreateWorldDamageText,
            OnGetPooledObject,
            OnReleasePooledObject,
            OnDestroyPooledObject,
            false,
            20,
            100
        );

        uiDamagePool = new ObjectPool<DamageText>(
            CreateUIDamageText,
            OnGetPooledObject,
            OnReleasePooledObject,
            OnDestroyPooledObject,
            false,
            20,
            100
        );

        
        //https://docs.unity3d.com/kr/2022.2/ScriptReference/Pool.ObjectPool_1-ctor.html
        
    }

    private void OnEnable()
    {
        GlobalEventManager.OnDamagePushed += HandleDamagePushed;
        
    }
    private void OnDisable()
    {
        GlobalEventManager.OnDamagePushed -= HandleDamagePushed;
    }
    
    
    private void HandleDamagePushed(Vector3 hitPoint, float damageAmount)
    {
        ShowDamage(useWorldDamageText,damageAmount, hitPoint);
    }
    
    public void ShowDamage(bool useWorld , float damage, Vector3 worldPos)
    {
        if (useWorld)
        {
            if (worldDamageTextPrefab == null) return;

            WorldDamageText wdt = worldDamagePool.Get();
            wdt.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
            wdt.Setup(damage);
            return;
        }
        
        if (damageTextPrefab == null || canvas == null || cam == null) return;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        DamageText dt = uiDamagePool.Get();
        RectTransform rect = dt.GetComponent<RectTransform>();
        rect.position = screenPos;
        dt.Setup(damage);
    }
    
    private WorldDamageText CreateWorldDamageText()
    {
        WorldDamageText obj = Instantiate(worldDamageTextPrefab, worldPoolParent);
        obj.SetPool(worldDamagePool);
        return obj;
    }

    private DamageText CreateUIDamageText()
    {
        DamageText obj = Instantiate(damageTextPrefab, uiPoolParent);
        obj.SetPool(uiDamagePool);
        return obj;
    }
    
    private void OnGetPooledObject<T>(T obj) where T : MonoBehaviour
    {
        obj.gameObject.SetActive(true);
    }

    private void OnReleasePooledObject<T>(T obj) where T : MonoBehaviour
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void OnDestroyPooledObject<T>(T obj) where T : MonoBehaviour
    {
        Destroy(obj.gameObject);
    }
    
    
    
    
}


/*
 * 유니티 내장 풀 << pool = new ObjectPool<T>(Createobj, OnGet,
 * OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize);
 * create: 풀에 오브젝트 없어서 새로 생성
 * Onget : 풀에 있는거 꺼낼떄
 * onRelease : 풀에 반납
 * onDestroy: 풀 꽉차서 버림
 * collectioncheck : 중복 반납
 * defaultcapacity : 기본 크기
 * maxsize : 최대 크기
 *
 * pool.Get() : 풀에서 오브젝트 꺼냄
 * pool.Release(obj) : 풀에 오브젝트 반납
 * 이 두개로 위쪽 함수들 알아서 호출
 *
 *
 * PoolObject<T> : 풀에서 관리할 옵젝
 * setPool(pool) : 어디 풀이랑 연결 시킬지
 * OnSpawned() : 풀에서 꺼낼 때 초기화
 * onDespawned() : 풀에 반납할때 코루틴같은거 정링
 * ReturnToPool: 반납할때어디다 release?
 * 
 */