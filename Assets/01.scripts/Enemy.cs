using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    
    NavMeshAgent pathfinder;
    private Transform target;
    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    private LivingEntity targetEntity;
    
    Material skinMaterial;
    Color originalColor;
    
    float nextAttackTime;
    
    float myCollisionRadius;
    float targetCollisionRadius;
    
    bool hasTarget;
    
    

    public enum State
    {
        Idle,
        Chasing,
        Attacking
    };
    State currentState;
    
    
    
    protected override void Start()
    {
        base.Start();
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            
            currentState = State.Chasing;
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath +=OntargetDeath;


            StartCoroutine(UpdatePath());
        
        
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        //pathfinder.SetDestination(target.position); << 비용 큼
        
        //Vector3.Distance(transform.position, target.position); //내 위치랑 타겟 위치 사이 거리 계산 << 제곱근 연산(root) <<비쌈
        
        if (!hasTarget)
        {
            return;
        }
        if (Time.time > nextAttackTime)
        {
            float sqrDstTotarget = (target.position - transform.position).sqrMagnitude;
            if (sqrDstTotarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());
            }
            
        }

        
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;
        Vector3 originalPosion = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);
        //Vector3 attackPosition = target.position;

        float percent = 0;
        float attackspeed = 3;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;
        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(1);
            }
            percent += Time.deltaTime * attackspeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosion, attackPosition, interpolation);
            // (x-x^2)4 || 0<x<1; 이러면 0->1->0 
            
            yield return null;
            
        }

        pathfinder.enabled = true;
        currentState = State.Chasing;
        skinMaterial.color = originalColor;

    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;
        
        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
                
            }

            
            yield return new WaitForSeconds(refreshRate);
        }
    }

    void OntargetDeath()
    {
        
        hasTarget = false;
        currentState = State.Idle;
        
    }
    
}
