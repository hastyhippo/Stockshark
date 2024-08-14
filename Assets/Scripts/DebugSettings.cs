using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSettings : MonoBehaviour
{
    public bool DebugCount;
    public bool DebugEvaluation;
    public bool DebugBranchingFactor;
    public bool DebugIterations;
    public bool DebugIterations2;
    public bool DebugIterations3;
    public bool DebugTime;
    public bool DebugTranspositionTableSize;
    public bool DebugDepth;
    public List<int> speed = new List<int>();

    public void Debug(int c, int d, string e, double b, int it, int it2, int it3, float t){
        string s = " ";
        if(DebugCount){
            s += " Move: " + c + " |";
        }
        if(DebugDepth){
            s += " Depth: " + d + " |";
        }
        if(DebugEvaluation){
            s += " Evaluation: " + e + " |";
        }
        if(DebugBranchingFactor){
            s += " Branching Factor: " + b + " |";
        }
        if(DebugIterations){
            s += " Iterations: " + it + " |";
        }
        if(DebugIterations2){
            s += " HalfIterations " + it2 + " |";
        }
        if(DebugIterations3){
            s += " QIterations " + it3 + " |";
        }
        if(DebugTime){
            s += " Time Elapsed: " + t + " ms " + " |";
        }
        speed.Add(it);
        print(s);
    }

    public void Debug(int c, int d, double e, double b, int it, float t){
        string s = " ";
        if(DebugCount){
            s += " Move: " + c + " |";
        }
        if(DebugDepth){
            s += " Depth: " + d + " |";
        }
        if(DebugEvaluation){
            s += " Evaluation: " + e + " |";
        }
        if(DebugBranchingFactor){
            s += " Branching Factor: " + b + " |";
        }
        if(DebugIterations){
            s += " Iterations: " + it + " |";
        }
        if(DebugTime){
            s += " Time Elapsed: " + t + " ms " + " |";
        }
        print(s);
    }
}
