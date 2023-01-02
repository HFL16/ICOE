using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class LiftMovementBH : BHNode
{
    
    Rigidbody rb;
    Transform transform;
    Transform target;

    int towardsOrAway;

    float maxSpeed;
    float acceleration;
    float brakeSpeed;

    float turnSpeed;

    Vector3 currentAcceleration;

    Vector3 relativePosition;

    public LiftMovementBH(Transform _transform, OrganismStats orgostats, int _towardsOrAway,ref Vector3 _currentAcceleration, Rigidbody _rb){ //currentAcceleration must come directly from movementSelector

        transform = _transform;
        rb = _rb;

        towardsOrAway = _towardsOrAway;

        turnSpeed = 0.1f;

        currentAcceleration = _currentAcceleration;

        maxSpeed = orgostats.maxSpeed;
        acceleration = orgostats.acceleration;
        brakeSpeed = 1f/acceleration * 2f;

    }

    public LiftMovementBH(Transform _transform, int _towardsOrAway,ref Vector3 _currentAcceleration, Rigidbody _rb, Dictionary<string,float> gParameters){

        transform = _transform;
        rb = _rb;

        towardsOrAway = _towardsOrAway;

        currentAcceleration = _currentAcceleration;

        turnSpeed = 0.1f;

        maxSpeed = gParameters["maxSpeed"];

        acceleration = gParameters["acceleration"];

        brakeSpeed = 16f*acceleration;

    }

    public override NodeState Evaluate(){

        if(target == null || !target.gameObject.activeInHierarchy){
            target = (Transform)parent.parent.parent.parent.GetData("target");

            if(target == null){
                state = NodeState.RUNNING;
                return state;
            }

            if(!target.gameObject.activeInHierarchy){
                parent.parent.parent.parent.SetData("target",null);
                state= NodeState.RUNNING;
                return state;
            }
        }

        Debug.Log(transform.name + " IS TRACKING");
        
        relativePosition = (target.position - transform.position) * towardsOrAway;

        Debug.Log(relativePosition.magnitude);
        
        transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(relativePosition),Time.deltaTime/turnSpeed);

        if(relativePosition.magnitude <= transform.lossyScale.z * 1.8f){
            Debug.Log("slwly");

            rb.velocity = Vector3.SmoothDamp(rb.velocity,relativePosition.normalized*0.01f,ref currentAcceleration,(10f/brakeSpeed) * Time.deltaTime);

            state = NodeState.SUCCESS;
            return state;
        }

        if(relativePosition.magnitude <= maxSpeed){
            rb.velocity = Vector3.SmoothDamp(rb.velocity,transform.forward*Mathf.Max(rb.velocity.magnitude/1.01f,0.05f),ref currentAcceleration,(10f/brakeSpeed) * Time.deltaTime);
            state = NodeState.SUCCESS;
            return state;
        }
        rb.velocity = Vector3.SmoothDamp(rb.velocity,transform.forward * maxSpeed,ref currentAcceleration,(10f/acceleration) * Time.deltaTime);

        state = NodeState.SUCCESS;
        return state;
    }

}
