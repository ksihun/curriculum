using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Gun : MonoBehaviour {

    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;
    public int burstCount;
    
    
    [FormerlySerializedAs("muzzle")] public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    float nextShotTime;

    public Transform shell;
    public Transform shellEjection;
    
    public MuzzleFlash muzzleflash;
    
    bool triggerReleaseSinceLastShot;
    int shotsRemainingInBurst;
    void Start() {
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
    }

    public void Shoot()
    {
        
        if (Time.time > nextShotTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }

                shotsRemainingInBurst--;
            }

            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleaseSinceLastShot)
                {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate (projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newProjectile.SetSpeed (muzzleVelocity);
            

            }
            Instantiate (shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate ();


        }
    }
    public void Shoot(float damage)
    {
        
        if (Time.time > nextShotTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }

                shotsRemainingInBurst--;
            }

            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleaseSinceLastShot)
                {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate (projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newProjectile.SetSpeed (muzzleVelocity);
                newProjectile.SetDamage (damage);
            

            }
            Instantiate (shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate ();


        }
    }


    public void OnTriggerHold()
    {
        Shoot();
        triggerReleaseSinceLastShot = false;


    }
    public void OnTriggerRelease()
    {
        triggerReleaseSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}