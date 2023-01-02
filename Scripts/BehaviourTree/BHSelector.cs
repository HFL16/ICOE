using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree{

    public class BHSelector : BHNode
    {

        public BHSelector() : base() {}
        public BHSelector(List<BHNode> _children) : base(_children) {}

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

}