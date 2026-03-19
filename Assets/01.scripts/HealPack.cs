using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPack : MonoBehaviour
{
    [SerializeField] float healAmount = 3;
    
    private void OnTriggerEnter(Collider other)
    {
        IHealable healable = other.GetComponent<IHealable>();
        if (healable != null)
        {
            healable.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
