using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class ScanSurroundingsBH : BHNode
{

    public static Dictionary<string,int> precedence = new Dictionary<string,int>(){ //this will be changed as complexity increases
        ["A"] = 1,
        ["B"] = 2,
        ["food"] = 3,
    };

    Transform transform;
    float lookRadius;
    float awareness; //awareness is how long the cooldown should be

    OrganismDiagnostics orgoDiags;
    

    public ScanSurroundingsBH(Transform _transform,OrganismStats orgostats){
        transform = _transform;
        lookRadius = orgostats.lookRadius;
        awareness = orgostats.awareness;
        orgoDiags = orgostats.orgoDiags;

        
    }

    public ScanSurroundingsBH(Transform _transform,OrganismDiagnostics _orgoDiags,Dictionary<string,float> gParams){

        transform = _transform;
        lookRadius = gParams["lookRadius"];
        awareness = gParams["awareness"];
        orgoDiags = _orgoDiags;

    }

    public override NodeState Evaluate(){

        if(orgoDiags.GetLastScan() < awareness || parent.parent.GetData("target") != null || !orgoDiags.openToReproduction){
            state=NodeState.RUNNING;
            return state;
        }

        Debug.Log("Scanned");

        Collider[] colliders = Physics.OverlapSphere(transform.position,lookRadius,~(1<<10));
                

        if(colliders.Length > 0){            

            int current = -1;
            int backup = -1;

            for(int i=0; i<colliders.Length;i++){

                if(current < 0){

                    if(colliders[i].name != transform.name){
                        current = i;
                        continue;
                    }
                    continue;
                }
                else{
                    if(precedence[colliders[i].tag] <= precedence[colliders[current].tag] && colliders[i].name != transform.name){
                    backup = current;
                    current = i;
                    }
                }



            }



            
            if(current < 0){
                state=NodeState.SUCCESS;
                return state;
            }
            if(colliders[current].name != transform.name){
                parent.parent.SetData("target",colliders[current].transform);

            }
            if(backup >= 0 && backup != current && colliders[backup].name != transform.name){
                parent.parent.SetData("backupTarget",colliders[backup].transform);
            }

            Debug.Log(transform.name + "'s target is now: " + ((Transform)parent.parent.GetData("target")).name);


        }



        orgoDiags.ResetLastScan();
        state = NodeState.SUCCESS;
        return state;


    }

}
