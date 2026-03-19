using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [Header("유닛 설정")]
    [SerializeField] private float attackDistanceThreshold = .5f;
    [SerializeField] private float timeBetweenAttacks = 1f;
    [SerializeField] private float attackSpeed = 3f;
    [SerializeField] private float damage = 1;


    
    protected override float AttackCooldown => timeBetweenAttacks;
    
    protected override bool IsInAttackRange()
    {

        float range = Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2);
        float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

        return sqrDstToTarget <= range;
    }
    
    public override void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColour)
    {
        
        pathfinder.speed = moveSpeed;

        if (hasTarget) {
            damage = Mathf.Ceil( targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        skinMaterial = GetComponent<Renderer> ().material;
        skinMaterial.color = skinColour;
        originalColor = skinMaterial.color;
        
    }
 
    
    protected override IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;
        Vector3 originalPosion = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);
        //Vector3 attackPosition = target.position;

        float percent = 0;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;
        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosion, attackPosition, interpolation);
            // (x-x^2)4 || 0<x<1; 이러면 0->1->0 
            
            yield return null;
            
        }

        pathfinder.enabled = true;
        currentState = State.Chasing;
        skinMaterial.color = originalColor;

    }

}
