using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class BehaviourDeployer : ScriptableObject
{

    static HashSet<string> exclusives = new HashSet<string>(new List<string>{"Movement","Feeding","Reproduction"});

    public static List<BHNode> DeployBehaviourPackage(Dictionary<string,List<OrganismGene>> genes, string requestedPackage, Transform transform, OrganismDiagnostics orgoDiags, ref Vector3 currentAcceleration,Rigidbody rb = null, GameObject organismPrefab = null, OrganismCoroutineHelper oCH = null, WorldSettings ws = null,HashSet<string> tagSet=null){
        
        List<BHNode> ls = new List<BHNode>();

        switch(requestedPackage){

            case "Sensory":
                
                ls = DeployBehavioursByGType(genes,"Sensory",transform,orgoDiags);

                break;

            case "Mating":

                ls.Add(DeployCheckerBehaviour(genes,"Mate",transform,orgoDiags,transform.lossyScale.z+0.8f,transform.tag)); 
                ls.Add(new BHSequence(new List<BHNode>{
                    DeployMovementBehaviour(genes,"Track",transform,orgoDiags, rb,ref currentAcceleration ,1, null), //false is to denote that this is not an idle movement
                    new SReproduceBH(genes,transform,orgoDiags,oCH,organismPrefab)
                }));

                break;

            case "Feeding":

                ls.Add(DeployCheckerBehaviour(genes,"Food",transform,orgoDiags,transform.lossyScale.z+0.8f,"food"));
                ls.Add(new BHSequence(new List<BHNode>{
                    DeployMovementBehaviour(genes,"Track",transform,orgoDiags, rb,ref currentAcceleration ,-1, null), //null is to denote that this is not an idle movement
                    new EatBH(transform,orgoDiags,genes["Feeding"][0].gParameters)
                }));

                break;

            case "Attack":

                ls.Add(DeployCheckerBehaviour(genes,"Attack",transform,orgoDiags,transform.lossyScale.z+0.8f,"prey",_tagSet : tagSet));
                ls.Add(new BHSequence(new List<BHNode>{
                    DeployMovementBehaviour(genes,"Track",transform,orgoDiags,rb, ref currentAcceleration,1,null),
                    new BHSelector(
                        DeployBehavioursByGType(genes,"Attack",transform,orgoDiags)    
                    )
                }));

                break;

            case "Defend":
                ls.Add(DeployCheckerBehaviour(genes,"Defend",transform,orgoDiags,transform.lossyScale.z+0.8f,"predator",_tagSet : tagSet));
                ls.Add(new BHSequence(new List<BHNode>{
                    DeployMovementBehaviour(genes,"Flee",transform,orgoDiags,rb, ref currentAcceleration,-1,null),
                    new BHSelector(
                        DeployBehavioursByGType(genes,"Defend",transform,orgoDiags)    
                    )
                }));

                break;


        }

        return ls;

    }

    public static List<BHNode> DeployBehavioursByGType(Dictionary<string,List<OrganismGene>> genes,string requestedGType , Transform transform,OrganismDiagnostics orgoDiags){ //probably will only be used by sensory, attack, and fleeing sequences

        List<BHNode> ls = new List<BHNode>();


        foreach(OrganismGene gene in genes[requestedGType]){

            switch(gene.gID){

                case "SSrn":
                    ls.Add(new ScanSurroundingsBH(transform,orgoDiags,gene.gParameters));
                    break;

                case "APnc":
                    ls.Add(new PunchBH(transform,orgoDiags,gene.gParameters));
                    break;


            }
            

        }

        return ls;


    }

    public static BHNode DeployCheckerBehaviour(Dictionary<string,List<OrganismGene>> genes, string checkerType, Transform transform, OrganismDiagnostics orgoDiags,float contactRange, string desiredTag,HashSet<string> _tagSet = null){

        switch(checkerType){

            case "Mate":
                return new CheckMateBH(transform, orgoDiags,genes["Sensory"][0].gParameters["lookRadius"],contactRange,desiredTag);

            case "Attack":
                return new CheckTagSetBH(transform,genes["Sensory"][0].gParameters["lookRadius"],contactRange,_tagSet,desiredTag);

            case "Defend":
                return new CheckTagSetBH(transform,genes["Sensory"][0].gParameters["lookRadius"],contactRange,_tagSet,desiredTag);

            default:
                return new CheckTagBH(transform,genes["Sensory"][0].gParameters["lookRadius"],contactRange,desiredTag);


        }

    }

    public static BHNode DeployMovementBehaviour(Dictionary<string,List<OrganismGene>> genes,string movementType  ,Transform transform, OrganismDiagnostics orgoDiags, Rigidbody rb, ref Vector3 currentAcceleration, int towardsOrAway, WorldSettings ws){ //if ws is null that means it's not idle movement

        OrganismGene movementGene = genes["Movement"][0];


        switch(movementGene.gID){

            case "MLft":

                switch(movementType){

                    case "Track":
                        return new LiftMovementBH(transform,towardsOrAway,ref currentAcceleration, rb, movementGene.gParameters);

                    case "Idle":
                        return new IdleLiftMovementBH(transform,orgoDiags,ref currentAcceleration, rb, ws, genes["Sensory"][0].gParameters["lookRadius"],movementGene.gParameters);

                    case "Flee":
                        return new LiftFleeBH(transform,towardsOrAway,ref currentAcceleration, rb, movementGene.gParameters,orgoDiags);

                    default:
                        return new LiftMovementBH(transform,towardsOrAway,ref currentAcceleration, rb, movementGene.gParameters);

                }

            default:
                return null;
        }

    }

    public static Dictionary<string,List<OrganismGene>> DeployDefaultGenes(){

        Dictionary<string,List<OrganismGene>> gDict = new Dictionary<string,List<OrganismGene>>(){

            ["Base"] = new List<OrganismGene>{new OrganismGene("Base","XBse",true,"Base",null)},

            ["Sensory"] = new List<OrganismGene>{new OrganismGene("ScanSurroundings","SSrn",true,"Sensory",null)}, 

            ["Attack"] = new List<OrganismGene>{new OrganismGene("Punch","APnc",false,"Attack",null)},

            ["Defend"]  = new List<OrganismGene>{},

            ["Feeding"] = new List<OrganismGene>{new OrganismGene("Eat","FEat",true,"Feeding",null)},

            ["Reproduction"] = new List<OrganismGene>{new OrganismGene("SReproduction","RSrp",true,"Reproduction",null)},

            ["Movement"] = new List<OrganismGene>{new OrganismGene("LiftMovement","MLft",true,"Movement",null)}

        };

        gDict["Base"][0].gParameters = new Dictionary<string,float>(){
            ["maxEnergy"] = 500f,
            ["maxHealth"] = 2f
        };

        gDict["Sensory"][0].gParameters = new Dictionary<string,float>(){
            ["awareness"] = 4f,
            ["lookRadius"] = 40f
        };

        gDict["Attack"][0].gParameters = new Dictionary<string,float>(){
            ["minDamage"] = 4f,
            ["maxDamage"] = 20f,
            ["attackSpeed"] = 2f
        };

        gDict["Movement"][0].gParameters = new Dictionary<string,float>(){
            ["maxSpeed"] = 4f,
            ["acceleration"] = 0.1f
        };

        gDict["Feeding"][0].gParameters = new Dictionary<string,float>(){
            ["eatSpeed"] = 2f,
            ["biteSize"] = 10f,
            ["eatEfficiency"]= 0.64f
        };

        gDict["Reproduction"][0].gParameters = new Dictionary<string,float>();

        return gDict;

        /*
        
        parameters

        base:
        maxEnergy
        maxHealth
        possibly size idk and would have to be split into size x y and z

        sensory:
        awareness
        lookRadius

        attack: (all genes must have)
        minDamage
        maxDamage
        attackSpeed

        movement:
        maxSpeed
        acceleration

        feeding:
        eatSpeed
        biteSize
        eatEfficiency

        reproduction:
        reproductionCooldown






        */




    }






    /*

        needed functions:

        DeployBehaviourBygID (used for non extraneous argument things like sensory, attack, and defen)
        DeployMovementBehaviour  (can include idle with a boolean parameter)
        DeployCheckerBehaviour


    */
    

    //Behaviour deployer should also have a dictionary of the different gId's corresponding to each jtype in other words (used for mutations)
    // in other words Dictionary<jtype,List<gIDs>>
    
    //And another dictinonary corresponding to each gId's default paramters (used for initial generation and mutations)
    //in other words Dictionary<gID,Dictionary of default parameters>

    //Naming convention for gID's:
    //gID's shouldn't exceed four characters, the first of which hints at its function and the subsequent abbreviate its name

    /*
    KEY FOR GID FIRST CHAR:
    M: Movement
    S: Sensory
    A: Attack
    D: Defence
    */

    //ls.Add(DeployBehaviourByGID(gene.GID,transform,orgoDiags,ref currentAcceleration)); an example of a default deployment




}
