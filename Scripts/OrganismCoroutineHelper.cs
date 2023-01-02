using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismCoroutineHelper : MonoBehaviour
{
    
    public void ActivateOrganismCoroutine(IEnumerator coroutine){
        StartCoroutine(coroutine);
    }


}
