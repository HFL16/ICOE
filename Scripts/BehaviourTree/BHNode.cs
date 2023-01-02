using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree{


    public enum NodeState{
        RUNNING,
        SUCCESS,
        FAILURE


    }

    public class BHNode{

        protected NodeState state;

        public BHNode parent;
        protected List<BHNode> children;

        private Dictionary<string,object> _dataContext;

        public BHNode(){
            parent = null;
            _dataContext = new Dictionary<string,object>();
            children = new List<BHNode>();
        }

        public BHNode(List<BHNode> _children){
            parent = null;
            _dataContext = new Dictionary<string,object>();
            children = new List<BHNode>();
            foreach(BHNode _child in _children){
                _Attach(_child);
            }
        }

        public void _Attach(BHNode node){ //TODO: Check if you can change children to a linked list
            node.parent = this;
            children.Add(node);
        }

        public virtual NodeState Evaluate() => NodeState.FAILURE;

        public void SetData(string key, object value){
            _dataContext[key] = value;
        }

        public object GetData(string key){

            object value = null;

            if(_dataContext.TryGetValue(key, out value)){
                return value;
            }

            BHNode current = parent;

            while(current != null){
                value = current.GetData(key);
                if (value != null){
                    return value;
                }
                current = current.parent;
            }

            return null;

        }

        public bool ClearData(string key){


            if(_dataContext.ContainsKey(key)){
                _dataContext.Remove(key);
                return true;
            }

            BHNode current = parent;

            while (current != null){
                bool cleared = current.ClearData(key);
                if (cleared){
                    return true;
                }
                current = current.parent;

            }

            return false;

        }

    }






}
