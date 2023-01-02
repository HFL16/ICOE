using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class PunchBH : BHNode
{

    Transform transform;

    float minDamage;
    float maxDamage;

    float punchCooldown;

    float range; //static and based on organism's size 

    OrganismDiagnostics orgoDiags;

    Transform lockedOnTarget;
    OrganismDiagnostics lockedOnTargetOrgoDiags;

    float damage; //stores last punch's damage


   
    public PunchBH(Transform _transform, OrganismStats orgostats){

        transform = _transform;

        minDamage = orgostats.minDamage;
        maxDamage = orgostats.maxDamage;

        punchCooldown = orgostats.attackSpeed;

        range = transform.lossyScale.z + 1.5f;

        orgoDiags = orgostats.orgoDiags;

        lockedOnTarget = null;
        lockedOnTargetOrgoDiags = null;

    }

    public PunchBH(Transform _transform, OrganismDiagnostics _orgoDiags, Dictionary<string,float> gParameters){

        transform = _transform;

        orgoDiags = _orgoDiags;

        minDamage = gParameters["minDamage"];
        maxDamage = gParameters["maxDamage"];

        punchCooldown = gParameters["attackSpeed"];

        range = transform.lossyScale.z + 1.5f;

        lockedOnTarget = null;
        lockedOnTargetOrgoDiags = null;

    }


    public override NodeState Evaluate(){

        if(orgoDiags.GetLastAttacked() <= punchCooldown){
            state = NodeState.FAILURE;
            return state;
        }

        Transform prey = (Transform)parent.parent.parent.GetData("inRange");

        if(prey == null){

            if(parent.parent.parent.GetData("prey") != null){
                state = NodeState.RUNNING;
                return state;

            }

            lockedOnTarget = null;
            lockedOnTargetOrgoDiags = null;
            state = NodeState.FAILURE;
            return state;
        }

        if(prey != lockedOnTarget){
            lockedOnTarget = prey;
            lockedOnTargetOrgoDiags = lockedOnTarget.GetComponent<OrganismBHT>().orgoDiags;
        }

        Vector3 relativePosition = lockedOnTarget.position-transform.position;

        if(relativePosition.magnitude >= range){
            state=NodeState.RUNNING;
            return state;
        }

        damage = Random.Range(minDamage,maxDamage);

        bool destroyed = lockedOnTargetOrgoDiags.DecreaseHealth(damage);
        Debug.Log(lockedOnTarget + "'s health is now" + lockedOnTargetOrgoDiags.GetHealth());

        if(destroyed){
            lockedOnTargetOrgoDiags.DestroyOrganism();
            parent.parent.parent.ClearData("prey");
            parent.parent.parent.ClearData("inRange");
            parent.parent.parent.parent.parent.ClearData("target");
        }

        orgoDiags.ResetLastAttacked();
        state= NodeState.SUCCESS;
        return state;




    }





}
