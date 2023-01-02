using UnityEngine;
using BehaviourTree;

public class UpdateCooldownsBH : BHNode
{

    OrganismDiagnostics orgoDiags;
    
    public UpdateCooldownsBH(OrganismStats orgostats){
        orgoDiags = orgostats.orgoDiags;
    }

    public UpdateCooldownsBH(OrganismDiagnostics _orgoDiags){
        orgoDiags = _orgoDiags;
    }

    public override NodeState Evaluate(){

        orgoDiags.IncrementLastEat(Time.deltaTime);
        orgoDiags.IncrementLastScan(Time.deltaTime);
        orgoDiags.IncrementLastReproduced(Time.deltaTime);
        orgoDiags.IncrementLastAttacked(Time.deltaTime);
        orgoDiags.IncrementLifeTimer(Time.deltaTime);
        orgoDiags.IncrementLastTolled(Time.deltaTime);

        state=NodeState.SUCCESS;
        return state; 

    }


}
