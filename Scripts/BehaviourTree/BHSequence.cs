using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BehaviourTree{

    public class BHSequence : BHNode
    {
        public BHSequence(): base(){}
        public BHSequence(List<BHNode> _children): base(_children){}

        public override NodeState Evaluate(){


            bool anyChildIsRunning = false;

            foreach(BHNode child in children){

                switch(child.Evaluate()){

                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;

                    case NodeState.SUCCESS:
                        continue;

                    case NodeState.RUNNING:
                    anyChildIsRunning = true;
                    continue;

                    default:
                    state = NodeState.SUCCESS;
                    return state;

                }

            }

            state = anyChildIsRunning ? NodeState.RUNNING: NodeState.SUCCESS;
            return state;



        }

    }


}