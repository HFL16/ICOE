using UnityEngine;
using BehaviourTree;
using System.Diagnostics;

public class DefaultIdleMovementBH : BHNode
{

    public static float speed = 5f;
    public static float radius = 10f;
    public static float proxima = 0.1f;
    public static float waitTime = 1f;
    public static float turnSpeed = 0.5f; //in seconds

    private Transform transform;
    private Vector3 target;

    private Vector3 worldCenter;
    private Vector3 worldDimensions;

    private float waiting;
    private float currentWaitTime;

    private float turning;
    private Quaternion startRotation;
    private Quaternion endRotation;
    

    public DefaultIdleMovementBH(Transform _transform, Vector3 _worldCenter, Vector3 _worldDimensions){
        transform = _transform;
        
        worldCenter = _worldCenter;
        worldDimensions = _worldDimensions;

        waiting = 0f;
        turning=0f;
        currentWaitTime = GetNewWaitTime();
        target = GetTarget();

    }

    public override NodeState Evaluate(){

        if(waiting < currentWaitTime){
            waiting += Time.deltaTime;
            state = NodeState.RUNNING;
            return state; 
        }

        Vector3 relativePosition = target - transform.position;
        

        if(relativePosition.magnitude <= DefaultIdleMovementBH.proxima){
            
            waiting = 0f;
            turning=0f;
            currentWaitTime = GetNewWaitTime();
            target = GetTarget();
            startRotation = transform.rotation;
            endRotation = Quaternion.LookRotation(target-transform.position);
        }
        else{
            if(turning< 1){
                transform.rotation = Quaternion.Slerp(startRotation,endRotation,turning/DefaultIdleMovementBH.turnSpeed);
                turning+= Time.deltaTime;
            }
            
            transform.position += Time.deltaTime * relativePosition.normalized * DefaultIdleMovementBH.speed;
        }


        state = NodeState.RUNNING;
        return state;
    }

    Vector3 GetTarget(){

        Vector3 t = new Vector3();
        Vector3 bounds = worldDimensions/2f ;
        Vector3 relPos = new Vector3();
        
        bool done = false;

        int c = 0;



        while(!done){

            t = Random.onUnitSphere * DefaultIdleMovementBH.radius + transform.position;


            if(bounds.x + worldCenter.x <= t.x
            || bounds.y + worldCenter.y <= t.y
            || bounds.z + worldCenter.z <= t.z
            || bounds.x - worldCenter.x >= t.x
            || bounds.y - worldCenter.y >= t.y
            || bounds.z - worldCenter.z >= t.z){
                continue;
            }

            relPos = t - transform.position;

            if(Physics.Raycast(transform.position,relPos.normalized,relPos.magnitude+0.01f)){
                continue;
            }

            done = true;

            c+=1;
            if(c>100){
                return new Vector3(0f,0f,0f);
            }

        }

        return t;

        

    }

    float GetNewWaitTime(){
        return DefaultIdleMovementBH.waitTime + Random.Range(-DefaultIdleMovementBH.waitTime/1.1f,DefaultIdleMovementBH.waitTime/4);
    }
    
}
