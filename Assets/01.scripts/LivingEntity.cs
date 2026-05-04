using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth = 5;
    public float health { get; protected set; }
    protected bool dead;
    public event System.Action OnDeath;
    public event System.Func<int> OnHealthChanged;
    protected virtual void Start()
    {
        health = startingHealth;
    }
    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    [ContextMenu("Self Destruct")]  
    protected virtual void Die()
    {
        dead = true;
        if (OnDeath != null)
        {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
    
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        //DamageSpawner.Instance.ShowDamage(damage, this.transform.position);
        GlobalEventManager.CallDamagePushed(this,transform.position,damage);
        if (health <= 0)
        {
            Die();
        }
    }


    
    

}
