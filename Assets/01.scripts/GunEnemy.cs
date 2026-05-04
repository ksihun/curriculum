using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunEnemy : EnemyBase
{
    [Header("유닛 설정")]
    [SerializeField] private float attackDistanceThreshold = 3f;
    [SerializeField] private float timeBetweenAttacks = 3f;
    [SerializeField] private float attackSpeed = 10f; //공격 모션? 시간
    [SerializeField] private float damage = 1;
    private GunController gunController;
    
    protected override float AttackCooldown => timeBetweenAttacks;
    
    protected override bool IsInAttackRange()
    {

        float range = Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2);
        float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

        return sqrDstToTarget <= range;
    }
    
    protected override void Awake()
    {
        base.Awake();
        gunController = GetComponent<GunController>();
    }
 
    
    protected override IEnumerator Attack()
    {
        currentState = State.Attacking;
        //pathfinder.enabled = false;
        Vector3 originalPosion = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);
        //Vector3 attackPosition = target.position;

        float percent = 0;

        skinMaterial.color = Color.blue;
        
        while (percent <= 1)
        {
            percent += Time.deltaTime * attackSpeed;
            
            yield return null;
            
        }
        if (target == null) yield break;
        Vector3 dir = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);

        gunController.Shoot(damage);
        //gunController.OnTriggerHold();
        
        
        //pathfinder.enabled = true;
        currentState = State.Chasing;
        skinMaterial.color = originalColor;
        
        
        
        

    }
    
    protected override Vector3 GetChaseDestination()
    {
        Vector3 awayDir = (transform.position - target.position).normalized;
        return target.position + awayDir * attackDistanceThreshold;
    }
    
    public override void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColour)
    {
        
        pathfinder.speed = moveSpeed;

        if (hasTarget) {
            damage = Mathf.Ceil( targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        skinMaterial = GetComponent<Renderer> ().sharedMaterial;
        skinMaterial.color = skinColour;
        originalColor = skinMaterial.color;
        
    }



}