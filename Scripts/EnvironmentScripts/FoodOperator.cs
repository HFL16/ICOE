using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodOperator : MonoBehaviour
{
    
    public float energyValue;

    public float GetEnergyValue(){
        return energyValue;
    }

    public bool DecrementEnergyValue(float decrement){

        energyValue-= decrement;

        return (energyValue <= 0) ? true : false;
        
    }

    public void DestroyFood(){
        gameObject.SetActive(false);
    }



}
