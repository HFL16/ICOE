using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class MovementSelector : BHNode
{

    Vector3 currentAcceleration;
    
    public MovementSelector(ref Vector3 _currentAcceleration): base() {
        currentAcceleration = _currentAcceleration;
    }

    public MovementSelector(List<BHNode> _children, ref Vector3 _currentAcceleration) : base(_children) {
        currentAcceleration = _currentAcceleration;
    }

    public override NodeState Evaluate(){

            foreach(BHNode child in children){

                switch(child.Evaluate()){

                    case NodeState.FAILURE:
                        continue;

                    case NodeState.SUCCESS:
                        state = NodeState.SUCCESS;
                        return state;

                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        continue;

                }

            }

            state = NodeState.FAILURE;
            return state;


        }


}
