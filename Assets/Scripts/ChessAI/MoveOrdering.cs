namespace Chess{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MoveOrdering : MonoBehaviour
    {
        public static List<Move> OrderMoves(Board board, List<Move> movesList, Move principleVar, ulong attackedSquares, ulong pawnAttackedSquares){
            List<Move> newList = new List<Move>();
            List<(Move, int)> tuples = new List<(Move, int)>();

            if(movesList.Contains(principleVar)){
                newList.Add(principleVar);
                movesList.Remove(principleVar);
            }
            else{
                print("Principal variation not found");
            }

            foreach(Move move in movesList){
                board.MakeMove(move);
                int eval = Evaluation.GetEvaluation(board) * -1;
                if(move.Type != 0){
                    if(move.Capture){
                        if((Evaluation.pieceValue[move.CapturedType] - Evaluation.pieceValue[move.Type]) >=0){
                            eval += 10000;
                        }
                        else{
                            if((attackedSquares & (1uL << move.TargetSquare)) != 0){
                                eval -= 10000;
                            }
                            else{
                                eval += 1000;
                            }
                        }
                    }
                    else if((pawnAttackedSquares & (1uL << move.TargetSquare)) != 0){
                        //print(move.MoveName + " attacked by pawn ");
                        eval -= 1000;
                    }
                    else if((attackedSquares & (1uL << move.TargetSquare)) != 0){
                        //print(move.MoveName + " attacked by piece");
                        eval -= 100;
                    }
                }

                tuples.Add((move,eval));
                board.UnmakeMove();
            }

            Functions.QuickSort(tuples,0, tuples.Count-1);
            tuples.Reverse();
            foreach((Move,int) tuple in tuples){
                newList.Add(tuple.Item1);
                //print(tuple.Item1.MoveName + " || " + tuple.Item2);
            }
 
            return newList;
        }
        public static List<Move> OrderMoves(Board board, List<Move> movesList, ulong attackedSquares, ulong pawnAttackedSquares){
            List<Move> newList = new List<Move>();
            List<(Move, int)> tuples = new List<(Move, int)>();

            foreach(Move move in movesList){
                board.MakeMove(move);
                int eval = Evaluation.GetEvaluation(board) * -1;
                if(move.Type != 0){
                    if(move.Capture){
                        if((Evaluation.pieceValue[move.CapturedType] - Evaluation.pieceValue[move.Type]) >=0){
                            eval += 10000;
                        }
                        else{
                            if((attackedSquares & (1uL << move.TargetSquare)) != 0){
                                eval -= 10000;
                            }
                            else{
                                eval += 1000;
                            }
                        }
                    }
                    else if((pawnAttackedSquares & (1uL << move.TargetSquare)) != 0){
                        //print(move.MoveName + " attacked by pawn ");
                        eval -= 1000;
                    }
                    else if((attackedSquares & (1uL << move.TargetSquare)) != 0){
                        //print(move.MoveName + " attacked by piece");
                        eval -= 100;
                    }
                }

                tuples.Add((move,eval));
                board.UnmakeMove();
            }

            Functions.QuickSort(tuples,0, tuples.Count-1);
            tuples.Reverse();
            foreach((Move,int) tuple in tuples){
                newList.Add(tuple.Item1);
                //print(tuple.Item1.MoveName + " || " + tuple.Item2);
            }
 
            return newList;
        }
    }


    //order:
    /*  PV-move of the principal variation from the previous iteration 
        Hash move from hash tables
        Winning captures/promotions
        Equal captures/promotions
        Killer moves (non capture), often with mate killers first
        Non-captures sorted by history heuristic and that like
        Losing captures
    */
}