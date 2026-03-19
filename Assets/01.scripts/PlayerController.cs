using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Vector3 velocity;
    Rigidbody myRigidBody;
    
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody>();
        
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
        
    }
    
    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);

    }

    
    // 정기적,짧은 반복 (프레임 영향 없ㅇ)
    private void FixedUpdate()
    {
        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);
        
        
        
    }

}
