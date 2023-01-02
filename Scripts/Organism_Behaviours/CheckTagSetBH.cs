using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class CheckTagSetBH : CheckTagBH
{

    HashSet<string> desiredTags;
    string tagIdentifier;


    public CheckTagSetBH(Transform _transform, OrganismStats orgostats, float _cRange, HashSet<string> _tagSet,string _tagIden) : base(_transform,orgostats,_cRange,""){

        desiredTags = _tagSet;
        tagIdentifier = _tagIden;

    }

    public CheckTagSetBH(Transform _transform,float _lookRadius, float _cRange, HashSet<string> _tagSet, string _tagIden) : base(_transform, _lookRadius, _cRange, "" ){

        desiredTags = _tagSet;
        tagIdentifier = _tagIden;

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

        if(desiredTags.Contains(target.tag)){

            

            relDist = target.position - transform.position;

            //Debug.Log(relDist.magnitude);

            if(relDist.magnitude < lookRadius){

                if(parent.GetData(tagIdentifier) == null){
                    parent.SetData(tagIdentifier,true);
                }

                if(relDist.magnitude > contactRange){
                    
                    state = NodeState.RUNNING;
                    return state;
                }


                parent.SetData("inRange",target);
                state = NodeState.SUCCESS;
                return state;

            }

            parent.ClearData(tagIdentifier);
            parent.ClearData("inRange");
            state = NodeState.FAILURE;
            return state;

        }
        state = NodeState.FAILURE;
        return state;

    }



    
}
