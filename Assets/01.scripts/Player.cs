using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerController))]
public class Player : LivingEntity , IHealable
{
    [SerializeField] private float moveSpeed = 3f;
    PlayerController _controller;
    Camera viewCamera;
    private GunController gunController;
    private bool invincible = false;
    private float invicibleTime = 0f;
    private Renderer r;
    
    public Crosshairs crosshairs;

    private void Awake()
    {
        r = GetComponent<Renderer>();
    }

    protected override void Start()
    {
        base.Start();
        _controller = GetComponent<PlayerController>();
        viewCamera = Camera.main;
        gunController = GetComponent<GunController>();


    }

    public override void TakeDamage(float damage)
    {
        if (invincible) return;
        base.TakeDamage(damage);
        StartCoroutine(isinvincible());
    }
    
    private IEnumerator isinvincible()
    {
        float timer = 0;
        invincible = true;
        while (timer <= invicibleTime)
        {
            r.material.color = Color.black;
            yield return new WaitForSeconds(0.05f);
            r.material.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            timer += 0.1f;

        }

        
        invincible = false;
    }

    
    void Update()
    {
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        _controller.Move(moveVelocity);
        
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        //Plane == 평면 (법선벡터,원점)
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;
        
        //Ray이 groundPlane과 충돌하면 true, 아니면 false
        
        //out == 포인터?
        
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin, point, Color.red);
            _controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);
        }


        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
    }
    
    public void Heal(float heal)
    {
        health = Mathf.Min(health + heal, startingHealth);
        Debug.Log(health);
        
    }
    

}
