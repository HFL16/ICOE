using System;
using System.Collections.Generic;
using UnityEngine;

public class OrganismGene: IComparable<OrganismGene>{

    public string gName;

    public string gID;

    public bool required;

    public string gType; 

    public Dictionary<string,float> gParameters;

    public OrganismGene(string _name, string _id, bool req , string _type, Dictionary<string,float> _gparams){
        gName=_name;
        gID = _id;
        required = req;
        gType = _type;
        gParameters = _gparams;
    }

    public override string ToString(){
        return ("Name: " + gName + "\n" + 
                "ID: " + gID + "\n" + 
                "Required: " + required + "\n" +
                "Type: " + gType + "\n" +
                "Parameter Count: " + gParameters.Count); 
    }

    public int CompareTo(OrganismGene other){
        return gID.CompareTo(other.gID);
    }

    public string GetParameters(){
        string ns = "";

        foreach(KeyValuePair<string,float> entry in gParameters){

            ns += entry.Key + ":  " + entry.Value;

        }
        return ns;

    }


}
