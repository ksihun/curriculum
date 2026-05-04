using TMPro;
using UnityEngine;

public class WorldDamageText : PoolObject<WorldDamageText>
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private Vector3 moveOffset = new Vector3(0, 1f, 0);
    private Camera cam;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshPro>();
        cam = Camera.main;
    }

    public void Setup(float damage)
    {
        OnSpawned();
        text.text = Mathf.RoundToInt(damage).ToString();
        StopAllCoroutines();
        StartCoroutine(ReturnAfterLifetime());
    }
    
    private System.Collections.IEnumerator ReturnAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        OnDespawned();
        ReturnToPool(this);
    }

    private void Update()
    {
        transform.position += moveOffset * Time.deltaTime;

        if (cam != null)
        {
            //transform.forward = cam.transform.forward;
            
            transform.LookAt(
                transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up
            );
        }
    }
}