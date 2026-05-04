using System;
using System.Collections.Generic;
using UnityEngine;


public static class GlobalEventManager
{

    public static event Action<Vector3, float> OnDamagePushed = delegate { };

    public static void CallDamagePushed(Vector3 hitPoint, float damageAmount)
    {   
        // OnDamagePushed?.Invoke(hitPoint, damageAmount);
        OnDamagePushed(hitPoint, damageAmount);
    }
    public static void CallDamagePushed(object sender, Vector3 hitPoint, float damageAmount)
    {
        if(!(sender is IDamageable))
            return;
        OnDamagePushed(hitPoint, damageAmount);

    }


    public static void CallDamagePushed<T>(T sender, Vector3 hitPoint, float dmg) where T : IDamageable
    {
        OnDamagePushed?.Invoke(hitPoint, dmg);
    }

}
