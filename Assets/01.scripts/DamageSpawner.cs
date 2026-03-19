using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageSpawner : MonoBehaviour
{
    public static DamageSpawner Instance { get; private set; }
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private Canvas canvas;
    private Camera cam;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cam = Camera.main;
    }
    
    
    public void ShowDamage(float damage, Vector3 worldPos)
    {
        if (damageTextPrefab == null || canvas == null) return;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        
        
        DamageText dt = Instantiate(damageTextPrefab, canvas.transform);
        RectTransform rect = dt.GetComponent<RectTransform>();
        rect.position = screenPos;
        dt.Setup(damage);
    }
    
    
    
}
