using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class IdleLiftMovementBH : BHNode{

    Rigidbody rb;
    Transform transform;
    Vector3 targetPoint;

    OrganismDiagnostics orgoDiags;


    float lookRadius;
    Vector3 worldDimensions;
    Vector3 worldCenter;

    float maxSpeed;
    float acceleration;
    float brakeSpeed;

    float turnSpeed;

    Vector3 currentAcceleration;

    Vector3 relativePosition;

    Quaternion targetRotation;

    float turnTime;
    float turnDelay;
    



    public IdleLiftMovementBH(Transform _transform, OrganismStats orgostats, ref Vector3 _currentAcceleration, Rigidbody _rb, WorldSettings worldSettings){

        transform = _transform;
        rb = _rb;

        currentAcceleration = _currentAcceleration;

        turnSpeed = 0.1f;

        lookRadius = orgostats.lookRadius;
        worldDimensions = worldSettings.worldDimensions;
        worldCenter = worldSettings.worldCenter;

        maxSpeed = orgostats.maxSpeed;
        acceleration = orgostats.acceleration;
        brakeSpeed = 16f*acceleration;

        targetPoint = GetNewTargetPoint();

        turnDelay = turnSpeed;
        turnTime=0f;
        

    }

    public IdleLiftMovementBH(Transform _transform, OrganismDiagnostics _orgoDiags, ref Vector3 _currentAcceleration, Rigidbody _rb,WorldSettings worldSettings ,float _lookRadius,Dictionary<string,float> gParameters){

        transform = _transform;
        rb = _rb;
        orgoDiags = _orgoDiags;

        currentAcceleration = _currentAcceleration;

        lookRadius = _lookRadius;

        worldDimensions = worldSettings.worldDimensions;

        worldCenter = worldSettings.worldCenter;

        turnSpeed = 0.1f;

        maxSpeed = gParameters["maxSpeed"];

        acceleration = gParameters["acceleration"];

        brakeSpeed = 16f*acceleration;
        

        targetPoint = GetNewTargetPoint();

        turnDelay = turnSpeed;
        turnTime=0f;
    }

    public override NodeState Evaluate(){
        
        relativePosition = targetPoint - transform.position;

        //Debug.Log(transform.name + " IS IDLY MOVING");

        if(relativePosition.magnitude <=0.1f){
            targetPoint = GetNewTargetPoint();
            turnTime = 0f;
        }

        relativePosition = targetPoint - transform.position;


        if(turnTime>turnDelay){

            transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(relativePosition),Time.deltaTime/turnSpeed);

            if(relativePosition.magnitude <= maxSpeed){
                rb.velocity = Vector3.SmoothDamp(rb.velocity,transform.forward*Mathf.Max(rb.velocity.magnitude/1.01f,0.1f),ref currentAcceleration,(10f/brakeSpeed) * Time.deltaTime);

                state = NodeState.SUCCESS;
                return state;
            }

            
            rb.velocity = Vector3.SmoothDamp(rb.velocity,transform.forward * maxSpeed,ref currentAcceleration,(10f/acceleration) * Time.deltaTime);

        }
        else{
            transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(relativePosition),Time.deltaTime/turnSpeed);
            rb.velocity = Vector3.SmoothDamp(rb.velocity,Vector3.zero,ref currentAcceleration,(10f/brakeSpeed) * Time.deltaTime);
        }
       

        turnTime+=Time.deltaTime;
        state = NodeState.SUCCESS;
        return state;

    }

    Vector3 GetNewTargetPoint(){

        Vector3 nt = new Vector3(0,2f,0);
        Vector3 bounds = worldDimensions/2f;
        Vector3 relPos = nt - transform.position;

        bool done = false;
        
        int c = 0;

        


        while(!done){
            nt = transform.position + Random.onUnitSphere * lookRadius;
            

            if(worldCenter.x  +  bounds.x<= nt.x
            || worldCenter.y  + bounds.y <= nt.y
            || worldCenter.z  + bounds.z <= nt.z
            || worldCenter.x  - bounds.x >= nt.x
            || worldCenter.y  - bounds.y >= nt.y
            || worldCenter.z  - bounds.z >= nt.z){
                continue;
            }

            relPos = nt - transform.position;

            if(Physics.Raycast(transform.position,relPos.normalized,relPos.magnitude+0.01f,1<<10)){
                continue;
            }

            done=true;

            c+=1;

            if(c>100){
                break;
            }

        }

        Debug.Log(transform.name + ": " + nt);

        return nt;



    }

    float GetNewWaitTime(){
        return turnDelay + Random.Range(-DefaultIdleMovementBH.waitTime/1.1f,DefaultIdleMovementBH.waitTime/4);
    }



}
