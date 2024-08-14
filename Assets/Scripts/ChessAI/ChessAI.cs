namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ChessAI : MonoBehaviour
    {
        public static int evalIterations;
        public static int halfIterations;
        public Search search;
        public MoveGenerator moveGen;
        public DebugSettings debugSettings;
        public int Depth;
        public static bool gameOver = false;
        public bool iterativeDeepening;
        public static int maxValue = 2147483647;
        public static int minValue = -2147483647;
        public Move Negamax(int depth, Board board){
            if(iterativeDeepening){
                return search.IterativeDeepening(board);
            }
            else{
                return search.NegaMax(board, Depth, minValue, maxValue).Item1;
            }
        }
    }}