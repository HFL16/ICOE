using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class LiftFleeBH : LiftMovementBH
{

    OrganismDiagnostics orgoDiags;


    public LiftFleeBH(Transform _transform, OrganismStats orgostats, int _towardsOrAway,ref Vector3 _currentAcceleration, Rigidbody _rb) : base(_transform,orgostats,_towardsOrAway,ref _currentAcceleration,_rb){
        orgoDiags = orgostats.orgoDiags;
    }

    public LiftFleeBH(Transform _transform, int _towardsOrAway,ref Vector3 _currentAcceleration, Rigidbody _rb, Dictionary<string,float> gParameters, OrganismDiagnostics _orgoDiags) : base(_transform,_towardsOrAway,ref _currentAcceleration, _rb,gParameters){
        orgoDiags = _orgoDiags;
    }

    

    public override NodeState Evaluate(){

        if(!orgoDiags.critical){
            state= NodeState.FAILURE;
            return state;
        }

        return base.Evaluate();

    }


    
}
