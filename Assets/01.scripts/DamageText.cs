using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class DamageText : PoolObject<DamageText>
{
    
    [SerializeField] private TMP_Text text;
    [SerializeField] private float moveUpSpeed = 1f;  
    [SerializeField] private float lifeTime = 0.5f; 
    
    private RectTransform rect;
    
    private float timer;
    


    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>(); 
        rect = GetComponent<RectTransform>();
            
    }
    
    

    public void Setup(float damage)
    {
        OnSpawned();
        timer = 0f;
        text.text = Mathf.RoundToInt(damage).ToString();
        
    }

    private void Update()
    {
        timer += Time.deltaTime;
        rect.anchoredPosition += Vector2.up * (moveUpSpeed * Time.deltaTime);

        if (timer >= lifeTime)
        {
            OnDespawned();
            ReturnToPool(this);
        }
    }
}
