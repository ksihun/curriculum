using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class Projectile : MonoBehaviour

{
    public LayerMask CollisionMask;
    float speed = 10; 
    float damage = 1;
    float lifetime = 3;
    float skinWidth = 0.1f; //충돌 여유 공간
    
    public Color trailCoulor;
     
    void Start()
    {
        Destroy(gameObject, lifetime);
        
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, CollisionMask);
        //생성 되는 순간 위치에 겹쳐있는 배열들 받아오기
        if (initialCollisions.Length > 0) //겹쳐있는게 하나라도 있으면
        {
            OnHitObject(initialCollisions[0],transform.position);//첫 겹쳐있는 콜라이더에게 데미지
            
        }
        
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailCoulor);
        
        
        
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
        GameObject.Destroy(gameObject);
        
    }

    public void SetDamage(float d)
    {
        damage = d;
    }
}
