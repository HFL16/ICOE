using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class SReproduceBH : BHNode
{

    Transform transform;

    GameObject orgoPrefab;

    float reproductionCooldown;

    public Dictionary<string,List<OrganismGene>> genes;

    Dictionary<string,List<OrganismGene>> outputGenes;

    static HashSet<string> exclusives = new HashSet<string>(new List<string>{"Base","Sensory","Feeding","Reproduction","Movement"});

    //consider making a set of static parameters that shouldn't be changed such as reproduction speed

    OrganismDiagnostics orgoDiags;

    OrganismStats orgoStats;

    OrganismCoroutineHelper orgoCH;

    Transform lockedOnMate;
    OrganismDiagnostics lockedOnMateOrgoDiags;
    Dictionary<string,List<OrganismGene>> otherGenes;

    bool doneReproducing;
    bool inProgress;

    static float reproductionDelay = 0.2f;

    public SReproduceBH(Transform _transform, OrganismStats orgostats,OrganismCoroutineHelper _orgoCH ,GameObject _orgoPrefab){
        transform = _transform;

        orgoPrefab = _orgoPrefab;

        genes = orgostats.genes;

        orgoDiags = orgostats.orgoDiags;

        orgoCH = _orgoCH;

        reproductionCooldown = orgostats.reproductionSpeed;

        createDebugGenes();
        orgoStats.genes = genes; 

        
    }

    public SReproduceBH(Dictionary<string,List<OrganismGene>> _genes, Transform _transform, OrganismDiagnostics _orgoDiags ,OrganismCoroutineHelper oCH, GameObject _orgoPrefab){

        genes = _genes;

        orgoDiags = _orgoDiags;

        transform = _transform;

        orgoCH = oCH;

        orgoPrefab = _orgoPrefab;

        reproductionCooldown = orgoDiags.GetReproductionCooldown();

    }


    public override NodeState Evaluate(){

        if(inProgress){
            state = NodeState.RUNNING;
            return state;
        }

        if(orgoDiags.GetReproducing() == null){
            
            if(parent.parent.GetData(transform.tag) != null){
                //Debug.Log(transform.name + " gochabich");
                state = NodeState.RUNNING;
                return state;
            }


            state = NodeState.FAILURE;
            return state;
        }

        if(orgoDiags.GetLastReproduced() <= reproductionCooldown){
            Debug.Log(transform.name + ": BFAIL");
            state = NodeState.FAILURE;
            return state;
        }

        Transform mate = (Transform)parent.parent.GetData("inRange");


        if(mate == null){
            Debug.Log(transform.name + ": CFAIL");
            lockedOnMate = null;
            lockedOnMateOrgoDiags = null;
            otherGenes = null;
            state = NodeState.FAILURE;
            return state;
        }
        Debug.Log("dspa");

        if(!mate.gameObject.activeInHierarchy){
            Debug.Log(transform.name + ": DFAIL");
            parent.parent.SetData("inRange",null);
            parent.parent.ClearData(transform.tag);
            lockedOnMate = null;
            lockedOnMateOrgoDiags = null;
            otherGenes = null;
            state = NodeState.FAILURE;
            return state;
        }

        if(lockedOnMate != mate){
            lockedOnMate = mate;
            OrganismBHT otherBHT = lockedOnMate.GetComponent<OrganismBHT>();
            lockedOnMateOrgoDiags = otherBHT.orgoDiags;
            otherGenes = otherBHT.genes;

        }


        if(doneReproducing){



            Debug.Log("donezo");

            parent.parent.ClearData("inRange");
            parent.parent.ClearData(transform.tag);
            parent.parent.parent.parent.ClearData("target");

            orgoDiags.ResetReproduction();
            lockedOnMateOrgoDiags.ResetReproduction();

            
            doneReproducing = false;

            CreateChild(outputGenes);

            state = NodeState.SUCCESS;
            return state;
        }

        if(!inProgress){

            inProgress = true;
            orgoCH.ActivateOrganismCoroutine(Reproduce());


        }

        state = NodeState.RUNNING;
        return state;

    }


    IEnumerator Reproduce(){

        Dictionary<string,List<OrganismGene>> newGenes = new Dictionary<string,List<OrganismGene>>();

        bool exclusive = false;

        Debug.Log(otherGenes);

        foreach(KeyValuePair<string,List<OrganismGene>> entry in genes){

            newGenes.Add(entry.Key,new List<OrganismGene>());

            exclusive = exclusives.Contains(entry.Key);

            if(exclusive){

                OrganismGene newGene = GetNewGene(genes[entry.Key][0],otherGenes[entry.Key][0]);

                if(newGene != null){
                    newGenes[entry.Key].Add(newGene);
                }

                yield return new WaitForSeconds(reproductionDelay);
                Debug.Log(entry.Key + " exclusive");

            }
            else{

                int i1 = 0;
                int i2 = 0;

                int comp = 0;

                while(i1 < genes[entry.Key].Count && i2 < otherGenes[entry.Key].Count){

                    comp = genes[entry.Key][i1].CompareTo(otherGenes[entry.Key][i2]);

                    if(comp < 0){

                        OrganismGene newGene = GetNewGene(genes[entry.Key][i1],null);

                        if(newGene != null){
                            newGenes[entry.Key].Add(newGene);
                        }

                        i1+=1;

                    }

                    else if(comp > 0){

                        OrganismGene newGene = GetNewGene(otherGenes[entry.Key][i2],null);

                        if(newGene != null){
                            newGenes[entry.Key].Add(newGene);
                        }

                        i2+=1;

                    }
                    else{

                        OrganismGene newGene = GetNewGene(genes[entry.Key][i1],otherGenes[entry.Key][i2]);

                        if(newGene != null){
                            newGenes[entry.Key].Add(newGene);
                        }

                        i1+=1;
                        i2+=1;

                    }

                    yield return new WaitForSeconds(reproductionDelay);

                }

                while(i1 < genes[entry.Key].Count){
                    OrganismGene newGene = GetNewGene(genes[entry.Key][i1],null);

                    if(newGene != null){
                        newGenes[entry.Key].Add(newGene);
                    }

                    i1+=1;
                    yield return new WaitForSeconds(reproductionDelay);
                }

                while(i2 < otherGenes[entry.Key].Count){
                    OrganismGene newGene = GetNewGene(otherGenes[entry.Key][i2],null);

                    if(newGene != null){
                        newGenes[entry.Key].Add(newGene);
                    }

                    i2+=1;
                    yield return new WaitForSeconds(reproductionDelay);
                }

            }

            /*

            if(Random.Range(0f,1f) < 0.02f){

                if(exclusives.Contains(entry.Key)){

                    newGenes[entry.Key][0] = INSERTMUTATIONCODE;

                }
                else{

                    newGenes[entry.Key].Add(INSERTMUTATIONCODE);
                    newGenes[entry.Key].Sort(delegate(OrganismGene x, OrganismGene y)){
                        return x.CompareTo(y);
                    }

                }

                yield return new WaitForSeconds(0.5f);

            }

            */

        }

        outputGenes = newGenes;
        inProgress = false;
        doneReproducing = true;

        yield return null;


    }


    public OrganismGene GetNewGene(OrganismGene p1, OrganismGene p2){

        if(p2 == null){

            if(p1.required){
                return new OrganismGene(p1.gName,p1.gID,p1.required,p1.gType,ModifyParamters(p1.gParameters));
            }

            return (Random.Range(0f,1f) < 0.98f) ? new OrganismGene(p1.gName,p1.gID,p1.required,p1.gType,ModifyParamters(p1.gParameters)) : null;

        }

        if(p1.gID != p2.gID){
            return (Random.Range(0f,1f) < 0.5f) ? new OrganismGene(p1.gName,p1.gID,p1.required,p1.gType,ModifyParamters(p1.gParameters)) : new OrganismGene(p2.gName,p2.gID,p2.required,p2.gType,ModifyParamters(p2.gParameters));
        }

        Dictionary<string,float> newParams = new Dictionary<string,float>();

        float sharePercent = 0f;

        foreach(KeyValuePair<string,float> entry in p1.gParameters){
            sharePercent = 0.5f + Random.Range(-0.2f,0.2f);

            newParams.Add(entry.Key, entry.Value * sharePercent + p2.gParameters[entry.Key] * (1f-sharePercent));
        }

        return new OrganismGene(p1.gName,p1.gID,p1.required,p1.gType,newParams);

    }

    public Dictionary<string,float> ModifyParamters(Dictionary<string,float> pParams){
        Dictionary<string,float> newParams = new Dictionary<string,float>();
        
        foreach(KeyValuePair<string,float> entry in pParams){
            newParams.Add(entry.Key ,entry.Value * (1f + Random.Range(-0.2f,0.2f)));
        }

        return newParams;
    }


    void CreateChild(Dictionary<string,List<OrganismGene>> childGenes){

        

        Vector3 childPos = transform.position + Random.onUnitSphere * 8f;
        Collider[] colliders = Physics.OverlapSphere(childPos, 4f);

        float c = 0f;

        while(colliders.Length > 0){
            childPos = transform.position + Random.onUnitSphere * (2f+c/0.25f);
            colliders = Physics.OverlapSphere(childPos, 1f);
            c+=0.5f;
        }

        //GameObject gChild = GameObject.Instantiate(orgoPrefab,childPos,Quaternion.identity);
        GameObject gChild = GameObject.Instantiate(orgoPrefab,transform.position +new Vector3(1,1,1),Quaternion.identity);


        OrganismBHT childBHT = gChild.GetComponent<OrganismBHT>();

        childBHT.SetGenes(childGenes);
        childBHT.BuildTree();

        

        string ns = "";


        foreach(KeyValuePair<string,List<OrganismGene>> entry in outputGenes){

            for(int g = 0; g<entry.Value.Count; g++){
                ns += entry.Value[g].gID + ", ";

            }

            ns+= " | ";
        }

        Debug.Log(ns);


    }


    public void createDebugGenes(){

        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        string jt = "";

        genes = new Dictionary<string,List<OrganismGene>>();

        for(int c = 0;c<3;c++){

            jt = chars[c].ToString();

            genes.Add(jt, new List<OrganismGene>());

            if(exclusives.Contains(jt)){
                string ns = "";

                for(int k = 0;k<4;k++){

                    ns+= chars[Random.Range(0,chars.Length-1)];
                
                }

                genes[jt].Add(new OrganismGene("aaa",ns,true,"T",new Dictionary<string,float>()) );

            }
            else{
                for(int g = 0;g<5;g++){

                    string ns = "";

                    for(int k = 0;k<4;k++){

                        ns+= chars[Random.Range(0,chars.Length-1)];
                    
                    }

                    genes[jt].Add(new OrganismGene("aaa",ns,true,"T",new Dictionary<string,float>()) );

                }

                genes[jt].Sort(delegate(OrganismGene x, OrganismGene y){

                    return x.CompareTo(y);

                });

            }
                
        }
    }


}

