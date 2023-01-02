using UnityEngine;
using BehaviourTree;

public class CheckMateBH : CheckTagBH
{


    Transform lockedOnMate;
    bool lockedOnMateApproved;
    OrganismDiagnostics lockedOnMateOrgoDiags;


    OrganismDiagnostics orgoDiags;

    public CheckMateBH(Transform _transform, OrganismStats orgostats,float _cRange,string _dtag): base(_transform,orgostats,_cRange,_dtag){


        lockedOnMate = null;
        lockedOnMateOrgoDiags = null;
        lockedOnMateApproved=false;
        orgoDiags = orgostats.orgoDiags;
    }

    public CheckMateBH(Transform _transform, OrganismDiagnostics _orgoDiags, float _lookRadius, float _cRange, string _dtag): base(_transform,_lookRadius,_cRange,_dtag){


        orgoDiags = _orgoDiags;
        lockedOnMate = null;
        lockedOnMateOrgoDiags = null;
        lockedOnMateApproved=false;

    }

    public override NodeState Evaluate(){

        //Debug.Log(transform.name + " REP: " + orgoDiags.GetReproducing());
        //Debug.Log(transform.name + " BRW: " + orgoDiags.GetBeingReproducedWith());
        //Debug.Log(parent.GetData("inRange"));

        
        
        if(parent.parent.parent.GetData("target") == null){
            
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)parent.parent.parent.GetData("target");



        if(!target.gameObject.activeInHierarchy){
            parent.parent.parent.SetData("target",null);
            state=NodeState.FAILURE;
            return state;

        }        
        
        if(target.tag == desiredTag){


            if(!orgoDiags.canReproduce){

                if(parent.parent.parent.GetData("backupTarget") != null){
                    parent.parent.parent.SetData("target",parent.parent.parent.GetData("backupTarget"));
                    
                }
                else{
                    Debug.Log(transform.name + " " +parent.parent.parent.GetData("target"));
                    parent.parent.parent.ClearData("target");
                }

                Debug.Log(transform.name + " realized they are a minor");
           
                state = NodeState.FAILURE;
                return state;
            }



            if(target != lockedOnMate){
                lockedOnMate = target;
                lockedOnMateOrgoDiags = target.GetComponent<OrganismBHT>().orgoDiags;

                lockedOnMateApproved = (lockedOnMateOrgoDiags.canReproduce && 
                lockedOnMateOrgoDiags.openToReproduction && 
                lockedOnMateOrgoDiags.GetReproductionCooldown() < lockedOnMateOrgoDiags.GetLastReproduced()); 

            }
            else{
                lockedOnMateApproved = (lockedOnMateOrgoDiags.openToReproduction && 
                lockedOnMateOrgoDiags.GetReproductionCooldown() < lockedOnMateOrgoDiags.GetLastReproduced()); 
            }

            //Debug.Log(transform.name + " REP: " + orgoDiags.GetReproducing());
            //Debug.Log(transform.name + " BRW: " + orgoDiags.GetBeingReproducedWith());
            


            if(!lockedOnMateApproved){
                
                if(lockedOnMateOrgoDiags.GetReproducing() == transform || lockedOnMateOrgoDiags.GetBeingReproducedWith() == transform){
                    //Debug.Log("tisq");
                    state=NodeState.RUNNING;
                    return state;
                }
            
                Debug.Log(transform.name + " cancelled their targetship, lost bc lomod.gr is " + lockedOnMateOrgoDiags.GetReproducing());
                Debug.Log(transform.name + " cancelled their targetship, lost bc lomod.gbrw is " + lockedOnMateOrgoDiags.GetBeingReproducedWith());

                lockedOnMate=null;
                lockedOnMateOrgoDiags = null;
                if(parent.parent.parent.GetData("backupTarget") != null){
                    parent.parent.parent.SetData("target",parent.parent.parent.GetData("backupTarget"));
                }
                else{
                    parent.parent.parent.ClearData("target");
                }

                state = NodeState.FAILURE;
                return state;

            }

            //Debug.Log(transform.name +": " + orgoDiags.GetReproducing());
            //Debug.Log(transform.name + ": " + orgoDiags.GetBeingReproducedWith());

            

            relDist = target.position - transform.position;
            //Debug.Log(relDist.magnitude);

            if(relDist.magnitude < lookRadius){
                //Debug.Log(transform.name + ": is here with " +parent.parent.parent.GetData("target"));

                if(parent.GetData(desiredTag) == null){
                    parent.SetData(desiredTag,true);

                }

                if(relDist.magnitude > contactRange){

                    state = NodeState.RUNNING;
                    return state;
                }

                if(orgoDiags.openToReproduction){
                    orgoDiags.SetReproducing(target);
                    lockedOnMateOrgoDiags.SetBeingReproducedWith(transform);
                    parent.SetData("inRange",target);
                }
                
                state = NodeState.SUCCESS;
                return state;

            }

            if(lockedOnMate != null){
                orgoDiags.SetReproducing(null);
                lockedOnMateOrgoDiags.SetBeingReproducedWith(null);
                lockedOnMate=null;
                lockedOnMateOrgoDiags = null;
            }

            parent.ClearData("inRange");
            parent.ClearData(desiredTag);
            state = NodeState.FAILURE;
            return state;


        }

        if(lockedOnMate != null){
            orgoDiags.SetReproducing(null);
            lockedOnMateOrgoDiags.SetBeingReproducedWith(null);
            lockedOnMate=null;
            lockedOnMateOrgoDiags = null;
        }
        state = NodeState.FAILURE;
        return state;

    }
    
}
