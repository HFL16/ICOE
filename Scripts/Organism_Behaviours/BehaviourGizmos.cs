using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourGizmos : MonoBehaviour
{

    public OrganismStats orgostats;
    public WorldSettings worldSettings;
    
    void OnDrawGizmos(){
        Gizmos.DrawWireCube(worldSettings.worldCenter,worldSettings.worldDimensions);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,orgostats.lookRadius);

    }


}
