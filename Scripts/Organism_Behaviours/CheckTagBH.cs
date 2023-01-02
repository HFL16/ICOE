using UnityEngine;
using BehaviourTree;

public class CheckTagBH : BHNode
{

    protected Transform transform;
    protected float lookRadius;
    
    protected Vector3 relDist;

    protected string desiredTag;

    protected float contactRange;

    public CheckTagBH(Transform _transform, OrganismStats orgostats,float _cRange,string _dtag){

        transform = _transform;
        lookRadius = orgostats.lookRadius;
        contactRange = _cRange;
        desiredTag = _dtag;


    }

    public CheckTagBH(Transform _transform, float _lookRadius, float _cRange, string _dtag){

        transform = _transform;
        lookRadius = _lookRadius;
        contactRange = _cRange;
        desiredTag = _dtag;

    }

    public override NodeState Evaluate(){

        

        if(parent.parent.parent.GetData("target") == null){

            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)parent.parent.parent.GetData("target");

        if(!target.gameObject.activeInHierarchy){
            parent.parent.parent.SetData("target",null);
            state = NodeState.FAILURE;
            return state;
        }        

        if(target.tag == desiredTag){

            relDist = target.position - transform.position;

            if(relDist.magnitude < lookRadius){

                if(parent.GetData(desiredTag) == null){
                    parent.SetData(desiredTag,true);
                }

                if(relDist.magnitude > contactRange){
                    //Debug.Log("hre");
                    state = NodeState.RUNNING;
                    return state;
                }

                
                

                parent.SetData("inRange",target);
                state = NodeState.SUCCESS;
                return state;

            }

            parent.ClearData(desiredTag);
            parent.ClearData("inRange");
            state = NodeState.FAILURE;
            return state;
            

        }
        state = NodeState.FAILURE;
        return state;

    }
    
}
