using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : PoolObject<Projectile>

{
    [SerializeField] float defaultDamage = 1;
    public LayerMask CollisionMask;
    float speed = 10; 
    float damage = 1;
    float lifetime = 3;
    float skinWidth = 0.1f; //충돌 여유 공간
    
    public Color trailCoulor;
    private TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }
        
        
    }
    public override void OnSpawned() //초기화
    {
        base.OnSpawned();

        StopAllCoroutines();
        StartCoroutine(LifeTimer());

        // 기본값으로 리셋
        damage = defaultDamage;

        // 트레일 초기화
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
            trail.transform.position = transform.position;
            trail.emitting = true;
        }

        // 생성 직후 겹침 체크
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, CollisionMask);
        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0], transform.position);
        }
    }

    public override void OnDespawned() //디스폰
    {
        base.OnDespawned();
        StopAllCoroutines();
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = false;
        }
    }
    
    private IEnumerator LifeTimer() 
    {
        yield return new WaitForSeconds(lifetime);

        OnDespawned();
        ReturnToPool(this);
    }

    public void SetSpeed(float newspeed)
    {
        speed = newspeed;
    }

    

    // Update is called once per frame
    void Update()
    {
        float moveDistance = Time.deltaTime * speed;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);

    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray,out hit, moveDistance + skinWidth, CollisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
        
    }



    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage,hitPoint , transform.forward);
        }
        //GameObject.Destroy(gameObject);
        
        OnDespawned(); //충돌 시 디스폰
        ReturnToPool(this); //반납
        
    }

    public void SetDamage(float d)
    {
        damage = d;
    }
}
