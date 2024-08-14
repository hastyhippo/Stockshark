namespace Chess{
   using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using System;

    public class Evaluation : MonoBehaviour
    {
        public MoveGenerator moveGen;
        public int maxQuieseDepth = 8;
        static int pawnValue = 100;
        static int knightValue = 320;
        static int bishopValue = 330;
        static int rookValue = 500;
        static int queenValue = 950;
        static int CastlingRightValue = 25;
        static int doubledPawnsPenalty = 30;
        static int isolatedPawnsPenalty = 25;
        static int passedPawnBonus = 50;
        public static int maxValue = 2147483647;
        public static int minValue = -2147483647;
        public int qIterations = 0;
        public bool quiesence;
        string bestLine;
        string moveRecord = "";

        public static Dictionary<int, int> offsets = new Dictionary<int, int>(){
                [8] = 0,
                [-8] = 1,
                [-1] = 2,
                [1] = 3,
                [7] = 4,
                [-7] = 5,
                [9] = 6,
                [-9] = 7
            };
        public static Dictionary<int, int> pieceValue = new Dictionary<int,int>(){
            [0] = pawnValue,
            [1] = knightValue,
            [2] = bishopValue,
            [3] = rookValue,
            [4] = queenValue,
            [5] = 10000,
            [6] = pawnValue,
            [7] = knightValue,
            [8] = bishopValue,
            [9] = rookValue,
            [10] = queenValue,
            [11] = 10000
        };

        public static Dictionary<int, char> pieceName = new Dictionary<int,char>(){
            [0] = 'P',
            [1] = 'N',
            [2] = 'B',
            [3] = 'R',
            [4] = 'Q',
            [5] = 'K',
            [6] = 'p',
            [7] = 'n',
            [8] = 'b',
            [9] = 'r',
            [10] = 'q',
            [11] = 'k'
        };
        public static int GetEvaluation(Board board){
            int evaluation = 0;
            ChessAI.halfIterations ++ ;

            int weight = board.whiteTurn ? 1 : -1;

            evaluation += GetMaterial(board);
            evaluation += GetPSQTValue(board, false);
            return evaluation * weight;
        }
        public int GetEvaluation(Board board, Move move){
            if(CheckRepetition(board)){
                return 0;
            }
            if(Draw(board)){
                return 0;
            }
            int evaluation = 0;
            ChessAI.evalIterations ++ ;

            int weight = board.whiteTurn ? 1 : -1;

            int castleRights = board.CastlingRights();
            int pieceCount = Functions.BitCount(board.pieces[1] | board.pieces[2] | board.pieces[3] | board.pieces[4] | board.pieces[5] | 
            board.pieces[7] | board.pieces[8] | board.pieces[9] | board.pieces[10] | board.pieces[11]); 

            evaluation += ((castleRights & 0b1) + ((castleRights & 0b10) >> 1) - ((castleRights & 0b100) >> 2) - ((castleRights & 0b1000) >> 3))*CastlingRightValue;

            evaluation += PawnEvaluation(board.pieces[0],board.pieces[6]);

            evaluation +=  (GetMaterial(board) + GetPSQTValue(board,pieceCount <= 4));
//quiesence ? qSearch(board, minValue, maxValue,maxQuieseDepth, "") :
            if(move.Capture){
                evaluation += GetSEE(board,move);
                // print("after SEE: " + evaluation);
                evaluation += pieceValue[move.CapturedType] * weight;
                // print("mateiral: " + (GetMaterial() - pieceValue[move.CapturedType] * -weight));
                // print("psqt: " + GetPSQTValue());
            }
            return evaluation * weight;
        }
        public static int PawnEvaluation(ulong whitePawns, ulong blackPawns){
            int eval = 0;
            ulong wPawnFile = 0uL;
            ulong bPawnFile = 0uL;
            for(int i = 0; i< 8;i++){
                int wPawns = Functions.BitCount(whitePawns & Precalculated.file[i]);
                int bPawns = Functions.BitCount(blackPawns & Precalculated.file[i]);

                eval += Mathf.Max(0,wPawns - 1) * doubledPawnsPenalty;
                eval -= Mathf.Max(0,bPawns - 1) * doubledPawnsPenalty;
                if(wPawns != 0){
                    wPawnFile |= Precalculated.file[i];
                }
                if(bPawns != 0){
                    bPawnFile |= Precalculated.file[i];
                }
            }
            // print("after doubled pawns: " + eval);
            // isolated pawns
            ulong wAttacked = 0uL;
            ulong bAttacked = 0uL;
            wAttacked |= (wPawnFile << 1 & ~Precalculated.file[0]) | (wPawnFile >> 1 & ~Precalculated.file[7]);
            bAttacked |= (bPawnFile << 1 & ~Precalculated.file[0]) | (bPawnFile >> 1 & ~Precalculated.file[7]);
            
            eval -= (Functions.BitCount(~wAttacked & whitePawns) - Functions.BitCount(~bAttacked & blackPawns)) * isolatedPawnsPenalty;
            // print("white isolated pawns: "+ Functions.BitCount(~wAttacked & whitePawns));
            // print("black isolated pawns: " + Functions.BitCount(~bAttacked & blackPawns));
            // print("isolated pawn penalty " + (Functions.BitCount(~wAttacked & whitePawns) - Functions.BitCount(~bAttacked & blackPawns)) * isolatedPawnsPenalty);

            //passed pawns
            int PPcount = 0;
            
            ulong wPP = whitePawns & ~(bAttacked | bPawnFile);
            while(wPP != 0){
                int i = Functions.pop_lsb(ref wPP);
                if((blackPawns & ~(wAttacked | wPawnFile) & Precalculated.bSquareAhead[i]) == 0){
                    PPcount--;
                }
            }

            ulong bPP = blackPawns & ~(wAttacked | wPawnFile);
            while(bPP != 0){
                int i = Functions.pop_lsb(ref bPP);
                if((blackPawns & ~(wAttacked | wPawnFile) & Precalculated.bSquareAhead[i]) == 0){
                    PPcount ++;
                }
            }

            eval += PPcount * passedPawnBonus;
            //print(eval);
            return eval;
        }
        string str = "";
        public (string,int) qSearch(Board board, int alpha, int beta, int depth){
            qIterations ++;

            int weight = board.whiteTurn? 1 : -1;
            int eval = GetMaterial(board);
            // int eval = GetEvaluation(board);

            if(depth == 0){
                //print(s + " || eval: " + GetEvaluation(board));
                return (moveRecord,eval);
            }
            if(CheckRepetition(board)){
                return (moveRecord,0);
            }
            //position sucks
            if(eval >= beta){
                return (moveRecord,beta);
            }
            if(eval > alpha){
                alpha = eval;
            }

            List<Move> movesList = moveGen.GenerateMoves(board, true);
            movesList = MoveOrdering.OrderMoves(board,movesList, moveGen.attackedSquares, moveGen.pawnAttackedSquares);

            string str = moveRecord;
            foreach(Move move in movesList){
                string p = move.MoveName + " ";
                moveRecord+= p;

                board.MakeMove(move);
                
                (string,int) tup;

                tup = qSearch(board, -beta, -alpha, depth-1);
                //print("tupitem1: " + tup.Item1 + " || depth: "  +depth + " || evaluation: " + tup.Item2);
                int score = -tup.Item2;

                board.UnmakeMove();

                if(score >= beta){
                    return (moveRecord,beta);
                }
                //print("alpha: " + alpha + " score: " + score);
                if(score >= alpha){
                    str = tup.Item1;
                    //print("str update: "+ str);
                    alpha = eval;
                }
                moveRecord = moveRecord.Remove(moveRecord.Length - p.Length);

            }
            //print("returning: " + str);
            return (str,alpha);
        }
        public static bool Draw(Board board){
            if(Functions.BitCount(board.pieces[0] | board.pieces[6]) != 0){
                return false;
            }
            if(Functions.BitCount(board.pieces[4] | board.pieces[3] | board.pieces[10] | board.pieces[9]) != 0){
                return false;
            }
            int wbishops = Functions.BitCount(board.pieces[2]);
            int wKnights = Functions.BitCount(board.pieces[1]);
            int bBishops = Functions.BitCount(board.pieces[8]);
            int bKnights = Functions.BitCount(board.pieces[7]);

            if((wbishops + wKnights == 0) || (wKnights + wbishops == 1)){
                if((bBishops + bKnights) == 0 || (bBishops + bKnights == 1)){
                    return true;
                }
            }
            return false;
        }
        public static int GetMaterial(Board board){
            return(Functions.BitCount(board.pieces[0])- Functions.BitCount(board.pieces[6])) * pawnValue + 
            (Functions.BitCount(board.pieces[1])- Functions.BitCount(board.pieces[7])) * knightValue + 
            (Functions.BitCount(board.pieces[2])- Functions.BitCount(board.pieces[8])) * bishopValue + 
            (Functions.BitCount(board.pieces[3])- Functions.BitCount(board.pieces[9])) * rookValue + 
            (Functions.BitCount(board.pieces[4])- Functions.BitCount(board.pieces[10])) * queenValue;
        }
        public static int GetPSQTValue(Board board,bool endgame){
            int value = 0;
            for(int i = 0; i < 6; i++){
                ulong u = board.pieces[i];
                while(u != 0){
                    int sq = Functions.pop_lsb(ref u);
                    value += (endgame && i == 5)? Precalculated.EG_WKingTable[sq] : Precalculated.PSQT[i][sq];
                }
            }
            for(int i = 6; i < 12; i++){
                ulong u = board.pieces[i];
                while(u != 0){
                    int sq = Functions.pop_lsb(ref u);
                    value -= (endgame && i == 11)? Precalculated.EG_BKingTable[sq] : Precalculated.PSQT[i][sq];
                }
            }
            return value;
        }
        public static bool CheckRepetition2(Board board){
            Move[] chainList = new Move[24];
            short c = 0;

            for(int i = 0; i< 24; i++){
                chainList[i] = new Move(true);
            }
            Stack<Move> MoveArchive = board.moveArchive;
            while(MoveArchive.Count > 0){
                Move m = MoveArchive.Pop();
                if(m.IsReversible){
                    for(int i =0; i<24; i++){
                        if((!chainList[i].isEmpty)&&(m.TargetSquare == chainList[i].StartSquare)){
                            if(m.StartSquare == chainList[i].TargetSquare){
                                if(c >= 1){
                                    return true;
                                }
                                chainList[i] = new Move(true);
                                continue;
                            }
                            chainList[i] = new Move(m.StartSquare,chainList[i].TargetSquare);
                            continue;
                        }
                    }
                    for(int i = 0;i < 24;i++){
                        if(chainList[i].isEmpty){
                            chainList[i] = m;
                            c++;
                        }
                    }
                }
                else{
                    break;
                }
            }
            return false;
        }
        public static bool CheckRepetition(Board board){
            if(board.ZobristArchive.Count == 0){
                print("noting in zobrist");
                return false;
            }
            ulong[] zobristArr = board.ZobristArchive.ToArray();
            Move[] moveArr = board.moveArchive.ToArray(); 
            Dictionary<ulong, int> repetitions = new Dictionary<ulong, int>();
            int i=0;
            foreach(ulong u in zobristArr){
                if(i < moveArr.Count()){
                    Move m = moveArr[i];
                    if(!m.IsReversible){
                        return false;
                    }
                    i++;
                }

                if(repetitions.ContainsKey(u)){
                    if(repetitions[u] < 2){
                        repetitions[u]++;
                    }else{
                        return true;
                    }
                }else{
                    repetitions.Add(u,1);
                }
            }
            return false;
        }
        public static int GetSEE(Board board, Move move){ 
            // IMPLEMENT KING CAPTURES
            //if black turn then white just captured black material 
            //if white turn then black just captured white material
            int w = board.whiteTurn ? -1 : 1;
            List<int> evaluations = new List<int>();

            //search through all attackers
            int score = pieceValue[move.CapturedType];
            int square = move.TargetSquare;

            #region Initiation
            ulong allPieces = board.pieces[0] | board.pieces[1] | board.pieces[2] | board.pieces[3] | board.pieces[4] | board.pieces[5] | 
            board.pieces[6] | board.pieces[7] | board.pieces[8] | board.pieces[9] | board.pieces[10] | board.pieces[11];

            ulong allPawns = board.pieces[0] | board.pieces[6], allQueens = board.pieces[4] | board.pieces[10], 
            allRooks = board.pieces[3] | board.pieces[9], allBishops = board.pieces[2] | board.pieces[8];
            ulong notDiagonal = board.pieces[0] | board.pieces[1] | board.pieces[3] | board.pieces[6] | board.pieces[7] | board.pieces[9];
            ulong gay = board.pieces[0] | board.pieces[1] | board.pieces[2] | board.pieces[6] | board.pieces[7] | board.pieces[8];
            ulong bishop = Precalculated.GetBishopAttacks(square, allPieces);
            ulong rook = Precalculated.GetRookAttacks(square, allPieces);


            //1. type , square
            List<(int, int)> wAttackers = new List<(int, int)>();
            List<(int, int)> bAttackers = new List<(int, int)>();
            #endregion
            #region Find Attackers
            ulong[] whiteAttackers = {
                Precalculated.pawnAttack[1][square] & board.pieces[0],
                Precalculated.knightMoves[square] & board.pieces[1],
                bishop & board.pieces[2],
                rook & board.pieces[3],
                (bishop | rook) & board.pieces[4],
                Precalculated.kingMoves[square] & board.pieces[5]
            };

            ulong[] blackAttackers = {
                Precalculated.pawnAttack[0][square] & board.pieces[6],
                Precalculated.knightMoves[square] & board.pieces[7],
                bishop & board.pieces[8],
                rook & board.pieces[9],
                (bishop | rook) & board.pieces[10],
                Precalculated.kingMoves[square] & board.pieces[11]
            };

            for(int i = 0; i < 6; i++){
                ulong u = whiteAttackers[i];
                while(u != 0){
                    int sq = Functions.pop_lsb(ref u);
                    wAttackers.Add((i, sq));
                }
            }

            for(int i = 0; i < 6; i++){
                ulong u = blackAttackers[i]; 
                while(u != 0){
                    int sq = Functions.pop_lsb(ref u);
                    bAttackers.Add((i, sq));
                }
            }
            wAttackers.Reverse();
            bAttackers.Reverse();
            #endregion

            bool white = board.whiteTurn;
            int currentPiece = pieceValue[move.Type];
            int type = move.Type;
            evaluations.Add(score);

            while(true){
                int weight = white ? 1 : -1;
                List<(int,int)> attackers = white ? wAttackers : bAttackers;

                //if no more attackers, break
                if(attackers.Count == 0){
                    break;
                }
                if(attackers[0].Item1%6 == 5){
                    List<(int,int)> defenders = white ? bAttackers : wAttackers;
                    if(defenders.Count != 0){
                        break;
                    }
                }
                (int,int) piece = attackers.Last();
                attackers.Remove(piece);
                score = currentPiece - score;
                evaluations.Add(score);

                //print("Capture: " + pieceName[piece.Item1] + "x" + pieceName[type] + "   ||   Evaluation " + score + " colour: " + white);
                currentPiece = pieceValue[piece.Item1];

                type = piece.Item1;
                white = !white;

                #region Find New Attackers
                bool g = false, d = false;
                //sort the attackers after finding new ones

                if(piece.Item1 % 6 == 4){
                    //queen
                    int x = piece.Item2- square;
                    if(Mathf.Abs(x) % 7 == 0 || Mathf.Abs(x) % 9 == 0){
                        d = true;
                    }
                    else{
                        g = true;
                    }
                }
                if(piece.Item1  % 6 == 0 || piece.Item1  % 6 == 2 || d){
                    int x = piece.Item2- square;
                    int dir;
                    if(x < 0){
                        if((-x) % 7 == 0){
                            dir = -7;
                        } else{
                            dir = -9;
                        }

                    }
                    else{
                        if(x % 7 == 0){
                            dir = 7;
                        } else{
                            dir = 9;
                        }
                    }
                    int directionIndex = offsets[dir];
                    for(int i = ((piece.Item2 - square)/dir) + 1; i <= Precalculated.DistanceToEdge[square][directionIndex]; i++){
                        ulong u = 1uL << (square + dir * i);
                        if((u & allPieces) == 0){
                            continue;
                        }
                        if((u & notDiagonal) != 0){
                            break;
                        }
                        if((u & board.pieces[2]) != 0){  //white bishop\\
                            wAttackers.Add((2,square + dir * i));
                            Functions.QuickSort(wAttackers, 0, wAttackers.Count-1);
                            break;
                        }
                        if((u & board.pieces[8]) != 0){ //black bishop\\
                            bAttackers.Add((8,square + dir * i));
                            Functions.QuickSort(bAttackers, 0, bAttackers.Count-1);
                            break;
                        }
                        if((u & board.pieces[4]) != 0){ //white queen\\
                            wAttackers.Add((4, square + dir * i));
                            Functions.QuickSort(wAttackers, 0, wAttackers.Count-1);
                            break;
                        }
                        if((u & board.pieces[10]) != 0){ //black queen\\
                            bAttackers.Add((10,square + dir * i));
                            Functions.QuickSort(bAttackers, 0, bAttackers.Count-1);
                            break;
                        }
                    }
                    continue;
                }
                if(piece.Item1 % 6 == 1){
                    //knight has no discoveries
                    continue;
                }
                if(piece.Item1 % 6 == 3 || g){
                    //rook
                    int dir;
                    int x = piece.Item2 - square;
                    if(x < 0){
                        if(-x < 8){
                            dir = -1;
                        } else{
                            dir = -8;
                        }
                    }else{
                        if(x < 8){
                            dir = 1;
                        } else{
                            dir = 8;
                        }
                    }

                    int directionIndex = offsets[dir];
                    for(int i = ((piece.Item2 - square)/dir) + 1; i <= Precalculated.DistanceToEdge[square][directionIndex]; i++){
                        ulong u = 1uL << (square + dir * i);
                        if((u & allPieces) == 0){
                            continue;
                        }
                        if((u & gay) != 0){
                            break;
                        }
                        if((u & board.pieces[3]) != 0){  //white rook\\
                            wAttackers.Add((3,square + dir * i));
                            Functions.QuickSort(wAttackers, 0, wAttackers.Count-1);
                            break;
                        }
                        if((u & board.pieces[9]) != 0){ //black rook\\
                            bAttackers.Add((9,square + dir * i));
                            Functions.QuickSort(bAttackers, 0, bAttackers.Count-1);
                            break;
                        }
                        if((u & board.pieces[4]) != 0){ //white queen\\
                            wAttackers.Add((4, square + dir * i));
                            Functions.QuickSort(wAttackers, 0, wAttackers.Count-1);
                            break;
                        }
                        if((u & board.pieces[10]) != 0){ //black queen\\
                            bAttackers.Add((10,square + dir * i));
                            Functions.QuickSort(bAttackers, 0, bAttackers.Count-1);
                            break;
                        }
                    }
                    continue;
                }
                if(piece.Item1 % 6 == 5){
                    break;
                }
                #endregion
            }
            int evaluation = evaluations.Last();
            for(int i = evaluations.Count-2; i >= 0; i--){
                evaluation = -Mathf.Max(-evaluations[i], evaluation);
            }
            return evaluation * w;
        }
    }
}
               