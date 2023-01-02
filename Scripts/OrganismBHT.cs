using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;


public class OrganismBHT : BHTree
{

    //IMPORTANT TODO: a lot of behaviours rely on orgostats for an organism's diagnostics. Put these diagnostics in the organism BHT to decouple it and change the references

    public WorldSettings worldSettings;
    public OrganismStats orgostats;
    public OrganismDiagnostics orgoDiags;
    public OrganismCoroutineHelper orgoCH;
    public GameObject orgoPrefab;
    public Rigidbody rigidBody;
    public bool debugTree;

    public Dictionary<string,List<OrganismGene>> genes;

    public HashSet<string> foreignPopulations = new HashSet<string>(new List<string>{"A","B"});

    protected override BHNode SetupTree(){


        if(!debugTree){
            return null;
        }

        orgostats.SetupStats();

        orgoDiags = orgostats.orgoDiags;
        
        Vector3  currentAcceleration = new Vector3();

        return new BHSequence(new List<BHNode>{
            new UpdateCooldownsBH(orgostats),
            new BHSequence(new List<BHNode>{
                new ScanSurroundingsBH(transform,orgostats)
            }),
            new MovementSelector(new List<BHNode>{
                new BHSequence(new List<BHNode>{
                    new CheckTagSetBH(transform,orgostats,1.8f,foreignPopulations,"predator"),
                    new BHSequence(new List<BHNode>{
                        new LiftFleeBH(transform,orgostats,-1,ref currentAcceleration,rigidBody),
                    })
                })
                //new IdleLiftMovementBH(transform,orgostats,ref currentAcceleration,rigidBody,worldSettings)
            },ref currentAcceleration)
        });  
        

    }

    BHNode buildDefaultTree(){

        Vector3  currentAcceleration = new Vector3();

        return new BHSequence(new List<BHNode>{
            new UpdateCooldownsBH(orgostats),
            new BHSequence(new List<BHNode>{
                new ScanSurroundingsBH(transform,orgostats)
            }),
            new MovementSelector(new List<BHNode>{
                new BHSequence(new List<BHNode>{
                    new CheckMateBH(transform,orgostats,1.8f,transform.tag),
                    new BHSequence(new List<BHNode>{
                        new LiftMovementBH(transform,orgostats,1,ref currentAcceleration,rigidBody),
                        new SReproduceBH(transform,orgostats,orgoCH,orgoPrefab)
                    })
                }),
                new BHSequence(new List<BHNode>{
                    new CheckTagBH(transform,orgostats,1.8f,"food"),
                    new BHSequence(new List<BHNode>{
                        new LiftMovementBH(transform,orgostats,1,ref currentAcceleration,rigidBody),
                        new EatBH(transform,orgostats)
                    })
                }),
                //new IdleLiftMovementBH(transform,orgostats,ref currentAcceleration,rigidBody,worldSettings)
            },ref currentAcceleration)
        }); 

    }


    public void BuildTree(){

        string ns = "";

        foreach(KeyValuePair<string,List<OrganismGene>> entry in genes){

            for(int g = 0; g<entry.Value.Count; g++){
                ns += entry.Value[g].gID + ", ";

            }

            ns+= " | ";
        }

        Debug.Log(ns);

        StartCoroutine(BuildTreeCoroutine());


    }

    IEnumerator BuildTreeCoroutine(){

        orgoDiags = new OrganismDiagnostics(genes,transform);
        Vector3 currentAcceleration = new Vector3();

        List<BHNode> SensoryBehaviours = BehaviourDeployer.DeployBehaviourPackage(genes,"Sensory",transform,orgoDiags, ref currentAcceleration);
        yield return new WaitForSeconds(0.2f);
        List<BHNode> MatingBehaviours = BehaviourDeployer.DeployBehaviourPackage(genes,"Mating",transform,orgoDiags, ref currentAcceleration, rb : rigidBody, organismPrefab : orgoPrefab, oCH : orgoCH);
        yield return new WaitForSeconds(0.2f);
        List<BHNode> EatingBehaviours = BehaviourDeployer.DeployBehaviourPackage(genes,"Feeding",transform,orgoDiags, ref currentAcceleration, rb : rigidBody);
        yield return new WaitForSeconds(0.2f);
        List<BHNode> AttackBehaviours = BehaviourDeployer.DeployBehaviourPackage(genes,"Attack",transform,orgoDiags, ref currentAcceleration, rb : rigidBody, tagSet : foreignPopulations);
        yield return new WaitForSeconds(0.2f);
        List<BHNode> DefendBehaviours = BehaviourDeployer.DeployBehaviourPackage(genes,"Defend",transform,orgoDiags, ref currentAcceleration, rb : rigidBody, tagSet : foreignPopulations);
        yield return new WaitForSeconds(0.2f);

        //movement behaviour
        //idle movement behaviour
        //eating behaviour
        //offensive behaviours
        //fleeing behaviours

        _root = new BHSequence(new List<BHNode>{
            new UpdateCooldownsBH(orgoDiags),
            new BHSequence(SensoryBehaviours),
            new MovementSelector(new List<BHNode>{
                new BHSequence(MatingBehaviours),
                new BHSequence(AttackBehaviours),
                new BHSequence(DefendBehaviours),
                new BHSequence(EatingBehaviours),
                //BehaviourDeployer.DeployMovementBehaviour(genes,"Idle",transform,orgoDiags,rigidBody ,ref currentAcceleration,0,worldSettings) 
            },ref currentAcceleration)



        });

        SphereCollider col = GetComponent<SphereCollider>();
        yield return new WaitForSeconds(0.4f);

        if(!col.enabled){
            col.enabled = true;
        }
        
        yield return null;

    }

    Dictionary<string,float> DecipherStats(){

        Dictionary<string,float> baseStats = new Dictionary<string,float>(){
            ["maxHealth"] = genes["Base"][0].gParameters["maxHealth"],
            ["maxEnergy"] = genes["Base"][0].gParameters["maxEnergy"],

            ["lifeToll"] = 0f

        };

        baseStats["lifeToll"] += (baseStats["maxHealth"] + baseStats["maxEnergy"])/128f;
        baseStats["lifeToll"] += genes["Sensory"][0].gParameters["lookRadius"]/2f;
        baseStats["lifeToll"] += 1f/genes["Sensory"][0].gParameters["awareness"];

        foreach(OrganismGene gene in genes["Attack"]){

            baseStats["lifeToll"] += (gene.gParameters["minDamage"]+gene.gParameters["maxDamage"])/8f;
            baseStats["lifeToll"] += 1f/gene.gParameters["attackSpeed"] * 8f;

        }

        //defense should be added to life toll once defenisve actions are developed

        baseStats["lifeToll"] += 1f/genes["Feeding"][0].gParameters["eatSpeed"];
        baseStats["lifeToll"] += genes["Feeding"][0].gParameters["biteSize"];
        baseStats["lifeToll"] += 1f/genes["Feeding"][0].gParameters["eatEfficiency"] * 8f;

        baseStats["lifeToll"] += 1f/genes["Reproduction"][0].gParameters["reproductionCooldown"] * 10f;

        baseStats["lifeToll"] += genes["Movement"][0].gParameters["maxSpeed"];
        baseStats["lifeToll"] += genes["Movement"][0].gParameters["acceleration"]*16f;

        return baseStats;





    }



    public void SetGenes(Dictionary<string,List<OrganismGene>> _genes){
        genes = _genes;
    }

}

/*

feed branch

new BHSequence(new List<BHNode>{
    new CheckTagBH(transform,orgostats,1.28f,"food"),
    new BHSelector(new List<BHNode>{
        new EatBH(transform,orgostats),
        new LiftMovementBH(transform,orgostats,1,ref currentAcceleration,rigidBody)
    })
})


mate branch

new BHSequence(new List<BHNode>{
                    new CheckMateBH(transform,orgostats,1.28f,transform.tag),
                    new BHSelector(new List<BHNode>{
                        new LiftMovementBH(transform,orgostats,1,ref currentAcceleration,rigidBody)
                    })
                })



*/
