using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismStats : MonoBehaviour
{

    public Dictionary<string,List<OrganismGene>> genes;

    public float maxHealth;

    public float maxEnergy;
    
    public float maxSpeed;

    public float acceleration;

    public float brakeSpeed;

    public float turnSpeed; //in seconds

    public float moveRadius;

    public float idleWaitTime;

    public float lookRadius;

    public float awareness; //cooldown for scanning surroundings

    public float eatSpeed; //how long between bites

    public float eatEfficiency; //how much is absorbed from each bite of food

    public float biteSize;

    public float reproductionSpeed; //how long between each reproduction

    public float minDamage;

    public float maxDamage;

    public float attackSpeed;

    public float lifeToll;

    public OrganismDiagnostics orgoDiags;

    public Vector3 worldCenter;
    public Vector3 worldDimensions;


    public void SetupStats(){
        orgoDiags = new OrganismDiagnostics(transform,maxHealth,maxEnergy,lifeToll);
    }

    public void OrganismCoroutineHelper(IEnumerator coroutine){
        StartCoroutine(coroutine);
    }


}
