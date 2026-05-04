using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolObject<T> : MonoBehaviour where T : MonoBehaviour
{
    protected ObjectPool<T> pool;
    private bool isReturned;

    public void SetPool(ObjectPool<T> pool) //풀 설정
    {
        this.pool = pool;
    }

    public virtual void OnSpawned() //풀 꺼낼때 호출
    {
        isReturned = false;
        gameObject.SetActive(true);
    }

    public virtual void OnDespawned() // 풀 넣을때 
    {
        StopAllCoroutines();
        //gameObject.SetActive(false);
    }

    protected void ReturnToPool(T obj) //반납
    {
        if (isReturned) return;
        isReturned = true;

        if (pool != null)
            pool.Release(obj);
        else
            Destroy(gameObject);
    }
}