using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : LivingEntity
{
    
    [Header("힐팩")]
    
    [Range(0f, 1f)] [SerializeField] private float healDropChance = 0.2f;
    [SerializeField] private GameObject healPackPrefab;
    
    public ParticleSystem  deathEffect;
    public static event System.Action OnDeathStatic;
    
    
    protected NavMeshAgent pathfinder;
    protected Transform target;
    protected LivingEntity targetEntity;
    
    protected Material skinMaterial;
    protected Color originalColor;
    
    protected float nextAttackTime;
    protected float myCollisionRadius;
    protected float targetCollisionRadius;
    
    protected bool hasTarget;
    
    public virtual void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
    {

    }
    
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health)
        {
            if (OnDeathStatic != null) OnDeathStatic();
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(
                Instantiate(deathEffect.gameObject, hitPoint,
                    Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
            
        }

        base.TakeHit(damage, hitPoint, hitDirection);
    }
    
    protected override void Die()
    {

        if (dead) return;
        
        TryDropHealPack();
        
        if (pathfinder != null)
            pathfinder.enabled = false;
        base.Die();
        
        
    }
    
    private void TryDropHealPack()
    {
        if (healPackPrefab is null) return;
        if (Random.value < healDropChance)
        {
            Instantiate(healPackPrefab, transform.position, Quaternion.identity);
        }
    }
    

    public enum State
    {
        Idle,
        Chasing,
        Attacking
    };
    protected State currentState;


    protected virtual void Awake()
    {
        //여기 바꾸니까 되긴함
        //skinMaterial = GetComponent<Renderer>().material;
        skinMaterial = GetComponent<Renderer>().sharedMaterial;
        originalColor = skinMaterial.color;
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {

            //currentState = State.Chasing;
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            // targetEntity.OnDeath +=OntargetDeath;


            // StartCoroutine(UpdatePath());


            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        }
    }
    
        
    protected override void Start()
    {
        base.Start();


        if (hasTarget)
        {
            
            currentState = State.Chasing;
            //hasTarget = true;
            // target = GameObject.FindGameObjectWithTag("Player").transform;
            // targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath +=OntargetDeath;


            StartCoroutine(UpdatePath());
        
        
            // targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
            // myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        //pathfinder.SetDestination(target.position); << 비용 큼
        
        //Vector3.Distance(transform.position, target.position); //내 위치랑 타겟 위치 사이 거리 계산 << 제곱근 연산(root) <<비쌈

        if (!hasTarget || dead)
        {
            return;
        }
        else TryAttack();
        


    }
    


    void TryAttack()
    {
        if (currentState == State.Attacking) return;
        if (Time.time < nextAttackTime) return;
        if (!IsInAttackRange()) return;
        
        nextAttackTime = Time.time + AttackCooldown;
        AudioManager.instance.PlaySound("Enemy Attack", transform.position);
        StartCoroutine(Attack());
        
    }
    
    protected abstract IEnumerator Attack();
    protected abstract bool IsInAttackRange();
    protected abstract float AttackCooldown { get; }
    
    protected virtual Vector3 GetChaseDestination()
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        return target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius);
    }
    

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;
        
        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                // Vector3 dirToTarget = (target.position - transform.position).normalized;
                // Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                if (!dead)
                {
                    pathfinder.SetDestination(GetChaseDestination());
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
