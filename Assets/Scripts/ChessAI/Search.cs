namespace Chess{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Diagnostics;

    public class Search : MonoBehaviour
    {
        public MoveGenerator moveGen;
        public ChessAI chessAI;
        public DebugSettings debugSettings;
        public Evaluation evaluation;
        public GameManager gameManager;
        public int castledKingValue;
        public static int prunedValue = -69696969;
        public static int minValue = -2147483647;
        public static int drawingValue = -69420;
        public static int winningValue = 1000000;
        public static int losingValue = -1000000;
        public static int nullMoveReduction = 1;
        public static Move zeroMove = new Move(1);
        public bool displayLines;
        private int depthSearchingTo;
        public float timeLimit;
        public bool moveOrdering;
        public int MaxDepth;
        public List<Board> boards = new List<Board>();
        public List<string> evals = new List<string>();
        private List<(Move, int)> evaluations = new List<(Move, int)>();
        public string bestLine;
        public List<float> timeTaken = new List<float>();
        bool searchCutOff = false;
        Stopwatch watch = new Stopwatch();
        string moveRecord = "";
        public static Dictionary<ulong, Move> moveDictionary = new Dictionary<ulong, Move>();
        public Move IterativeDeepening(Board board){
            searchCutOff = false;
            watch.Start();
            bestLine = "";

            int weight = board.whiteTurn ? 1 : - 1;
            int depth = 1;
            int score = 0;
            Move bestMove = new Move(true);


            while(depth <= MaxDepth){
                if(watch.ElapsedMilliseconds >= timeLimit){
                    watch.Stop();
                    break;
                }

                depthSearchingTo = depth;
                (Move,int) searchResult = depth > 1? NegaMax(board, depth, ChessAI.minValue, ChessAI.maxValue, false, bestMove) : 
                NegaMax(board, depth, ChessAI.minValue, ChessAI.maxValue, true, bestMove); 

                int new_score = searchResult.Item2;
                Move move = searchResult.Item1;

                if(score >= winningValue){
                    bestMove = move;
                    break;
                }

                if(!searchCutOff){
                    score = new_score;
                    bestMove = move;
                }
                else{
                    depth--;
                    break;
                }
                depth++;

            }
            #region evaluation display
            string pr = "";
            if(score == drawingValue || score == -drawingValue || (score % winningValue == 0 && score != 0)){
                if(score == drawingValue || score == -drawingValue){
                    pr = "Draw";
                }
                else if(score > 0){
                    pr = "Mate in " + (depth - (score/1000000));
                }
                else{
                    pr = "Mated in " + (depth + (score/1000000) + 1);
                }
            }
            else{
                pr = (System.Math.Round((float)((float)score/100f), 2) * weight).ToString();
            }

            if(board.whiteTurn){
                evaluations.Reverse();
            }
            debugSettings.Debug(GameManager.moveCount, depth, pr, System.Math.Round(System.Math.Pow(ChessAI.evalIterations,
             (double)1/depth),2), ChessAI.evalIterations, ChessAI.halfIterations, evaluation.qIterations, watch.ElapsedMilliseconds);
            #endregion
            watch.Reset();
            if(displayLines){
                print(bestLine);
            } 
            return bestMove;
        }
        public (Move,int) NegaMax(Board board, int depth, int alpha, int beta){
            boards.Clear();
            evaluations.Clear();
            evals.Clear();
            watch.Reset();
            watch.Start();

            int weight = board.whiteTurn ? 1 : - 1;
            ChessAI.evalIterations = 0;
            ChessAI.halfIterations = 0;
            Move bestMove = new Move(true);
            int max = minValue;

            List<Move> moves = moveGen.GenerateMoves(board, false);
            if(moves.Count == 0){
                ChessAI.gameOver = true;
            }

            if(moveOrdering){
                moves = MoveOrdering.OrderMoves(board,moves, moveGen.attackedSquares, moveGen.pawnAttackedSquares);
            }
            depthSearchingTo = depth;

            string Line = "";
            foreach(Move move in moves){
                string pr = move.MoveName + " ";
                moveRecord+= pr;

                board.MakeMove(move);
                (string,int) tup = NigaMax(depth-1, board, -beta, -alpha, false);
                int score = -tup.Item2;

                if(move.Flag == 2 && score!= max){
                    score += castledKingValue;
                }
                //boards.Add(new Board(board));
            
                #region display
                if(score != max || score >= winningValue){
                    evaluations.Add((move, score));
                    // print("MOVE: " + move.MoveName + "    ||   score: " + score + "  ||  max: " + max + "  ||  bestmove: " + bestMove.MoveName);
                }
                else{
                    evaluations.Add((move,prunedValue));
                }
                #endregion

                if(score > max){
                    Line = tup.Item1;
                    bestMove = move;
                    max = score;
                }
                board.UnmakeMove();

                moveRecord = moveRecord.Remove(moveRecord.Length - pr.Length);

                if(score >= winningValue){
                    bestMove = move;
                    break;
                }
                alpha = Mathf.Max(alpha, score);   
            }
            // if(!moveDictionary.ContainsKey(zob1)){
            //     moveDictionary.Add(zob1,bestMove);
            // }else{
            //     moveDictionary.Remove(zob1);
            //     moveDictionary.Add(zob1,bestMove);
            // }
            #region display
            Functions.QuickSort(evaluations, 0, evaluations.Count - 1);
            evaluations.Reverse();
            foreach((Move, int) tup in evaluations){
                int value = tup.Item2;
                string str = tup.Item1.MoveName;
                
                if(tup.Item1.Flag == 2){
                    if(tup.Item1.TargetSquare % 8 < 4){
                        str= "O-O-O";
                    }
                    else{
                        str = "O-O";
                    }
                }
                if(value == drawingValue || value == -drawingValue||value == prunedValue || (value % winningValue == 0 && value != 0)){
                    if(value == prunedValue){
                        evals.Add(str + "  |  Pruned");
                    }
                    else if(value == drawingValue || value == -drawingValue){
                        evals.Add(str + "  |  Draw");
                    }
                    else if(value > 0){
                        evals.Add(str + "  |  Mate in " + (depth - (value/1000000)));
                    }
                    else{
                        evals.Add(str + "  |  Mated in " + (depth + (value/1000000)));
                    }
                }
                else{
                    evals.Add(str + "  |  " + value);
                }
            }
            string p = "";
            if(max == drawingValue || max == -drawingValue || (max % winningValue == 0 && max != 0)){
                if(max == drawingValue|| max == -drawingValue ){
                    p = "Draw";
                }
                else if(max > 0){
                    p = "Mate in " + (chessAI.Depth - (max/1000000) + 1);
                }
                else{
                    p = "Mated in " + (chessAI.Depth + (max/1000000) + 1);
                }
            }
            else{
                p = (System.Math.Round((float)((float)max/100f), 2) * weight).ToString();
            }
            #endregion
            
            if(board.whiteTurn){
                evaluations.Reverse();
            }
            debugSettings.Debug(GameManager.moveCount, depth, p, System.Math.Round(System.Math.Pow(ChessAI.evalIterations, (double)1/depth),2), ChessAI.evalIterations, 
            ChessAI.halfIterations, evaluation.qIterations, watch.ElapsedMilliseconds);
            timeTaken.Add( watch.ElapsedMilliseconds);
            watch.Reset();
            if(displayLines){
                print(Line);
            }

            // foreach(KeyValuePair<ulong, Move> asd in moveDictionary){
            //     print(asd.Key + " || " + asd.Value.MoveName);
            // }
            // string str_ = "";
            // for(int i = 0;i < depth-1; i++){
            //     print("hash: " + Functions.HashPosition(board));
            //     Move m = moveDictionary[Functions.HashPosition(board)];
            //     str_ += m.MoveName;
            //     board.MakeMove(m);
            //     print(" | move sequence: " + str_);

            // }
            // for(int i = 0;i< depth-1;i++){
            //     board.UnmakeMove();
            // }

            return (bestMove, max);
        }
        public (Move,int) NegaMax(Board board, int depth, int alpha, int beta, bool depth1, Move PV){
            boards.Clear();
            evaluations.Clear();
            moveRecord = "";
            // moveDictionary.Clear();

            int weight = board.whiteTurn ? 1 : - 1;
            ChessAI.evalIterations = 0;
            ChessAI.halfIterations = 0;
            Move bestMove = new Move(true);
            int max = minValue;

            List<Move> moves = moveGen.GenerateMoves(board, false);
            if(moves.Count == 0){
                ChessAI.gameOver = true;
            }
            
            if(moveOrdering){
                moves = depth1? MoveOrdering.OrderMoves(board, moves, moveGen.attackedSquares, moveGen.pawnAttackedSquares) : MoveOrdering.OrderMoves(board, moves, PV, moveGen.attackedSquares, moveGen.pawnAttackedSquares);
            }

            depthSearchingTo = depth;
            string Line = "";

            foreach(Move move in moves){
                string p = move.MoveName + " ";
                moveRecord+= p;

                board.MakeMove(move);
                //boards.Add(new Board(board));

                (string,int) tup = NigaMax(depth-1, board, -beta, -alpha, true);
                int score = -tup.Item2;

                if(move.Flag == 2 && score != max){
                    score += castledKingValue;
                }

                if(score != max || score >= winningValue){
                    evaluations.Add((move, score));
                    // print("MOVE: " + move.MoveName + "    ||   score: " + score + "  ||  max: " + max + "  ||  bestmove: " + bestMove.MoveName);
                }
                else{
                    evaluations.Add((move,prunedValue));
                }

                if(score > max){
                    Line = tup.Item1;
                    bestMove = move;
                    max = score;
                }
                moveRecord = moveRecord.Remove(moveRecord.Length - p.Length);

                board.UnmakeMove();

                alpha = Mathf.Max(alpha, score);

                if(watch.ElapsedMilliseconds >= timeLimit){
                    searchCutOff = true;
                    return(bestMove,0);
                    //adding break here spawns king at 0,64
                }
            }
            bestLine = Line;           
            Functions.QuickSort(evaluations, 0, evaluations.Count - 1);

            evaluations.Reverse();
            evals.Clear();

            foreach((Move, int) tup in evaluations){
                int value = tup.Item2;
                string str = tup.Item1.MoveName;

                if(tup.Item1.Flag == 2){

                    if(tup.Item1.TargetSquare % 8 < 4){
                        str= "O-O-O";
                    }
                    else{
                        str = "O-O";
                    }
                }
                if(value == drawingValue|| value ==-drawingValue ||value == prunedValue || (value % winningValue == 0 && value != 0)){
                    if(value == prunedValue){
                        evals.Add(str + "  |  Pruned");
                    }
                    else if(value == drawingValue || value == -drawingValue){
                        evals.Add(str + "  |  Draw");
                    }
                    else if(value > 0){
                        evals.Add(str + "  |  Mate in " + (depth - (value/1000000) - 1));
                    }
                    else{
                        evals.Add(str + "  |  Mated in " + (depth + (value/1000000) - 1));
                    }
                }
                else{
                    evals.Add(str + "  |  " + value);
                }
            }
            
            return (bestMove, max);
        }
        public (string,int) NigaMax(int depth, Board board, int alpha, int beta, bool iterative){
            int weight = board.whiteTurn ? 1 : -1;

            #region CheckTerminalNode
            if(depth == 0){
                if(moveGen.TerminalNode(board))
                {
                    if(moveGen.inCheck){
                        return  (moveRecord,-1000000);
                    }
                    else{
                        return (moveRecord,drawingValue);
                    }
                }
                return (moveRecord, evaluation.GetEvaluation(board, board.moveArchive.Peek()));
            }
            List<Move> moves = moveGen.GenerateMoves(board, false);
            if(moves.Count == 0){
                if(moveGen.inCheck){
                    return  (moveRecord,-1000000 * (depth + 1));
                }
                else{
                    return (moveRecord,0);
                }
            }
            #endregion
            
            moves = moveOrdering ? MoveOrdering.OrderMoves(board, moves, moveGen.attackedSquares, moveGen.pawnAttackedSquares) :moves;

            int max = minValue;
            // ulong zob1 = Functions.HashPosition(board);
            string str = "";

            foreach(Move move in moves){
                string pr = move.MoveName + " ";
                moveRecord+= pr;
      
                board.MakeMove(move);

                (string,int) res = NigaMax(depth-1, board, -beta, -alpha, iterative);
                int score = -res.Item2;

                // max = Mathf.Max(score, max);
                
                if(score > max){
                    str = res.Item1;
                    max = score;
                }

                board.UnmakeMove();

                moveRecord = moveRecord.Remove(moveRecord.Length - pr.Length);

                alpha = Mathf.Max(alpha, max);
  
                if(alpha >= beta){
                    return (str, beta);
                }
                if(watch.ElapsedMilliseconds >= timeLimit && iterative){
                    searchCutOff = true;
                    break;                    
                }
            }
            return(str,max);
        }
        public bool CanNullMove(Board board){
            return true;
        }
    }
}
            //Null Move Heuristic\\
            // if(!moveGen.inCheck && CanNullMove(board) && depth - nullMoveReduction - 1 > 0){
            //     board.whiteTurn = !board.whiteTurn;
            //     print("depth: " + (depth - nullMoveReduction - 1) + "  a: " + -beta + "  b: " + (-beta + 1));
            //     int value = -NigaMax(depth - nullMoveReduction - 1, board, -beta, -beta + 1, false);
            //     if(value >= beta){
            //         gameManager.GetFen(board);
            //         print("returned!   " + value);
            //         return value;
            //     }
            // }