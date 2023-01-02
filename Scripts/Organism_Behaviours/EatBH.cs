using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class EatBH : BHNode
{
    Transform transform;

    float eatCooldown;
    float biteSize;
    float eatEfficiency;

    float lastEat;

    Transform lockedOnFoodTransform;
    FoodOperator lockedOnFood;

    OrganismDiagnostics orgoDiags;

    public EatBH(Transform _transform, OrganismStats orgostats){

        transform = _transform;

        eatCooldown = orgostats.eatSpeed;
        eatEfficiency = orgostats.eatEfficiency;
        biteSize = orgostats.biteSize;
        orgoDiags = orgostats.orgoDiags;

    }

    public EatBH(Transform _transform, OrganismDiagnostics _orgoDiags,Dictionary<string,float> gParameters){

        transform = _transform;

        eatCooldown = gParameters["eatSpeed"];
        eatEfficiency = gParameters["eatEfficiency"];
        biteSize = gParameters["biteSize"];

        orgoDiags = _orgoDiags;

    }

    public override NodeState Evaluate(){

        if(orgoDiags.GetLastEat() <= eatCooldown){
            
            state= NodeState.FAILURE;
            return state;
        }

        Transform food = (Transform)parent.parent.GetData("inRange");

        if(food == null){

            if(parent.parent.GetData("food") !=null){
                
                state = NodeState.RUNNING;
                return state;
            }

            


            lockedOnFood= null;
            lockedOnFoodTransform = null;
            state = NodeState.FAILURE;
            return state;
        }

        

        if(lockedOnFoodTransform != food){
            lockedOnFoodTransform = food;
            lockedOnFood = food.GetComponent<FoodOperator>();
        }

        if(lockedOnFood.GetEnergyValue() <=0){
            lockedOnFood.DestroyFood();
            parent.parent.ClearData("food");
            parent.parent.ClearData("inRange");
            parent.parent.parent.parent.ClearData("target");
            state = NodeState.FAILURE;
            return state;

        }

        //Debug.Log("paha");

        orgoDiags.IncreaseEnergy(Mathf.Min(biteSize,lockedOnFood.GetEnergyValue()) * eatEfficiency);
        bool destroyed = lockedOnFood.DecrementEnergyValue(Mathf.Min(biteSize,lockedOnFood.GetEnergyValue()));
        Debug.Log(orgoDiags.GetEnergy());

        if(destroyed){
            lockedOnFood.DestroyFood();
            parent.parent.ClearData("food");
            parent.parent.ClearData("inRange");
            parent.parent.parent.parent.ClearData("target");
        }


        Debug.Log("hegemony");
        orgoDiags.ResetLastEat();
        state = NodeState.SUCCESS;
        return state;

        
    }


}
