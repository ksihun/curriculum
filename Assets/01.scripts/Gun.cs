//using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.Pool;


public class Gun : MonoBehaviour {

    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;
    public int burstCount;
    
    [FormerlySerializedAs("muzzle")] public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int projectilesPerMag;
    public float reloadTime = .3f;
    
    [Header("반동")]
    public Vector2 kickMinMax = new Vector2(0.05f, 0.2f);
    public Vector2 recoilAngleMinMax = new Vector2(3, 5); 
    public float recoilMoveSettleTime = .1f;
    public float recoilRotationSettleTime =.1f;
    
    
    

    [Header("Effects")]
    float nextShotTime;
    public Transform shell;
    public Transform shellEjection;
    public MuzzleFlash muzzleflash;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;
    
    
    bool triggerReleaseSinceLastShot;
    int shotsRemainingInBurst;
    int projectilesRemainingInMag;
    bool isReloading;
    
    Vector3 recoilSmoothDampVelocity;
    float recoilAngle;
    float recoilRotSmoothDampVelocity;
    
    private ObjectPool<Projectile> projectilePool; //총알 풀
    private Transform projectilepoolParent; //총알 풀 부모
    
    
    
   
    
    private void Awake()
    {
        projectilePool = new ObjectPool<Projectile>(
            CreateProjectile, //생성 될떄
            OnGetPooledObject, //꺼낼 떄
            OnReleasePooledObject, //반납 
            OnDestroyPooledObject, //꽉차서 버림
            false,
            20,
            50
        );
        projectilepoolParent = new GameObject("ProjectilePool").transform;
    }

    void Start() {
        muzzleflash = GetComponent<MuzzleFlash>();
        
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }

    private void LateUpdate()
    {
        transform.localPosition =
            Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;
        
        if (!isReloading && projectilesRemainingInMag == 0) {Reload();}
    }
    
    public void Reload()
    {
        
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
        
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;

        Vector3 initalRot = transform.localEulerAngles;
        float maxReloadAngle = 30;
        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initalRot + Vector3.left * reloadAngle;
            yield return null;
        }
        
        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    public void Shoot()
    {
        
        if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
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
                if (projectilesRemainingInMag == 0) break;
                projectilesRemainingInMag--;
                nextShotTime = Time.time + msBetweenShots / 1000f;
                Projectile newProjectile = projectilePool.Get();
                newProjectile.transform.SetPositionAndRotation(projectileSpawn[i].position, projectileSpawn[i].rotation);
                newProjectile.OnSpawned();
                newProjectile.SetSpeed(muzzleVelocity);
                newProjectile.SetDamage(1f);
            }
            Instantiate (shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate ();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
            
            AudioManager.instance.PlaySound(shootAudio, transform.position);


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

            nextShotTime = Time.time + msBetweenShots / 1000f;

            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                Projectile newProjectile = projectilePool.Get(); //꺼내고
                newProjectile.transform.SetPositionAndRotation(projectileSpawn[i].position, projectileSpawn[i].rotation); //위치 설정
                newProjectile.OnSpawned(); //총알 초기화
                newProjectile.SetSpeed(muzzleVelocity); //속도
                newProjectile.SetDamage(damage); //데미지
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

    public void Aim(Vector3 aimPoint)
    {
        if (isReloading) return;
        transform.LookAt(aimPoint);
    }
    
    private Projectile CreateProjectile() //풀 빔
    {
        Projectile obj = Instantiate(projectile, projectilepoolParent);
        obj.SetPool(projectilePool);
        return obj;
    }
    
    private void OnGetPooledObject(Projectile obj) //풀 꺼냄
    {
        obj.gameObject.SetActive(true);
    }
    
    private void OnReleasePooledObject(Projectile obj) //풀 리턴
    {
        obj.gameObject.SetActive(false);
        //obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
    
    private void OnDestroyPooledObject(Projectile obj) // 풀 꽉 참
    {
        Destroy(obj.gameObject);
    }
    
    
}