using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree{

    public abstract class BHTree: MonoBehaviour{

    protected BHNode _root = null;

    protected void Start(){
        _root = SetupTree();
    }

    private void Update(){
        if (_root != null){
            _root.Evaluate();
        }

    }

    protected abstract BHNode SetupTree();



}



}
