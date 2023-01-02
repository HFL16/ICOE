using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismDiagnostics
{


    Transform transform;

    //livings

    float health;
    float energy;
    float lifeTimer;
    float lifeToll;


    //reproduction
    Transform reproducing; //both are expressed as other parent
    Transform beingReproducedWith;

    float reproductionCooldown; //will not vary across organisms 
    float reproductionMaturity; //also will not vary

    //stats

    float maxHealth;
    float maxEnergy;

    //accessors

    public bool hungry{
        get{return energy / maxEnergy <= 0.64f;}
    }

    public bool critical{
        get{return health/maxHealth <= 0.32f;}
    }

    public bool canReproduce{
        get{return lifeTimer > reproductionMaturity;}
    }

    public bool openToReproduction{
        get{return ((reproducing == null) && (beingReproducedWith == null));}
    }

    //cooldowns
    float lastEat;
    float lastScan;
    float lastReproduced;
    float lastAttacked;
    float lastTolled;

    public OrganismDiagnostics(Transform _transform,float _maxHealth, float _maxEnergy,float _lifeToll){

        transform=_transform;

        maxHealth = _maxHealth;
        maxEnergy = _maxEnergy;
        health = maxHealth/4f;
        energy = maxEnergy/4f;

        reproducing = null;
        beingReproducedWith = null;
        reproductionCooldown = 4f;
        reproductionMaturity = 4f; //will be increased after debugging

        lastEat=0f;
        lastScan=0f;
        lastReproduced=0f;
        lastAttacked = 0f;
        lastTolled = 0f;

        lifeTimer=0f;
        lifeToll = _lifeToll;

    }

    public OrganismDiagnostics(Dictionary<string,List<OrganismGene>> genes, Transform _transform){

        transform = _transform;
        
        maxHealth = genes["Base"][0].gParameters["maxHealth"];

        maxEnergy = genes["Base"][0].gParameters["maxEnergy"];

        lifeToll = 0f;

        lifeToll += (maxHealth + maxEnergy)/128f;
        lifeToll += genes["Sensory"][0].gParameters["lookRadius"]/2f;
        lifeToll += 1f/genes["Sensory"][0].gParameters["awareness"];

        foreach(OrganismGene gene in genes["Attack"]){

            lifeToll += (gene.gParameters["minDamage"]+gene.gParameters["maxDamage"])/8f;

        }

        //defense should be added to life toll once defenisve actions are developed

        lifeToll += 1f/genes["Feeding"][0].gParameters["eatSpeed"];
        lifeToll += genes["Feeding"][0].gParameters["biteSize"];
        lifeToll += 1f/genes["Feeding"][0].gParameters["eatEfficiency"] * 8f;

        lifeToll += genes["Movement"][0].gParameters["maxSpeed"];
        lifeToll += genes["Movement"][0].gParameters["acceleration"]*16f;

        health = maxHealth;
        energy = maxEnergy;

        reproducing = null;
        beingReproducedWith = null;
        reproductionCooldown = 8f;
        reproductionMaturity = 10f; //will be increased after debugging

        lastEat=0f;
        lastScan=0f;
        lastReproduced=0f;
        lastAttacked = 0f;

        lifeTimer=0f;

        Debug.Log(lifeToll);

    }

    public float GetHealth(){
        return health;
    }

    public float GetEnergy(){
        return energy;
    }

    public float GetMaxHealth(){
        return maxHealth;
    }

    public float GetMaxEnergy(){
        return maxEnergy;
    }

    public float GetLastEat(){
        return lastEat;
    }

    public float GetLastScan(){
        return lastScan;
    }

    public float GetLastReproduced(){
        return lastReproduced;
    }

    public float GetLastAttacked(){
        return lastAttacked;
    }

    public float GetLastTolled(){
        return lastTolled;
    }

    public Transform GetReproducing(){
        return reproducing;
    }

    public Transform GetBeingReproducedWith(){
        return beingReproducedWith;
    }

    public float GetReproductionCooldown(){
        return reproductionCooldown;
    }


    //cooldown increments

    public void IncrementLastEat(float delta){
        lastEat+=delta;
    }

    public void IncrementLastScan(float delta){
        lastScan += delta;
    }

    public void IncrementLastReproduced(float delta){
        lastReproduced += delta;
    }

    public void IncrementLastAttacked(float delta){
        lastAttacked+=delta;
    }

    public void IncrementLastTolled(float delta){
        lastTolled += delta;
    }

    public void IncrementLifeTimer(float delta){
        lifeTimer += delta;

        if(lifeTimer % 8f <= 0.05 && lastTolled > 4){
            LifeToll();
            ResetLastTolled();
        }

    }

    //cooldown resets

    public void ResetLastEat(){
        lastEat=0f;
    }

    public void ResetLastScan(){
        lastScan=0f;
    }

    public void ResetLastReproduced(){
        lastReproduced = 0f;
    }

    public void ResetLastAttacked(){
        lastAttacked = 0f;
    }

    public void ResetLastTolled(){
        lastTolled = 0f;
    }

    public void ResetReproduction(){
        lastReproduced = 0f;
        reproducing = null;
        beingReproducedWith = null;
    }

    //Stat increases and decrease

    public void IncreaseEnergy(float delta){ //does not account for maximum
        energy = Mathf.Min(energy + delta, maxEnergy);
    }

    public void IncreaseHealth(float delta){
        health = Mathf.Min(health + delta, maxHealth);
    }

    public bool DecreaseHealth(float delta){

        if(!openToReproduction){
            return false;
        }

        health -= delta;
        return health<=0;
    }

    public bool DecreaseEnergy(float delta){
        if(!openToReproduction){
            return false;
        }

        energy = Mathf.Max(0f,energy -= delta);
        return energy<=0;

    }

    //reproduction

    public void SetReproducing(Transform _transform){
        reproducing = _transform;
    }

    public void SetBeingReproducedWith(Transform _transform){
        beingReproducedWith = _transform;
    }

    

    public void SetMaxHealth(float _mh){
        maxHealth = _mh;
    }

    public void SetMaxEnergy(float _me){
        maxEnergy = _me;
    }

    // Life stuff

    public void DestroyOrganism(){
        transform.gameObject.SetActive(false);
    }

    public void LifeToll(){
        if(DecreaseEnergy(lifeToll)){
            if(DecreaseHealth(health*0.25f)){
                DestroyOrganism();
            }
        }
    }



}
