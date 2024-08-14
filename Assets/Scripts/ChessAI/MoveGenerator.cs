namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class MoveGenerator : MonoBehaviour
    {
        List<Move> moves;
        bool whiteTurn;
        int pawnThing;
        int fIndex;
        int eIndex;
        int fInd;
        int eInd;
        int weight;

        public bool inCheck;
        bool doubleCheck;

        ulong[] pieces;
        ulong friendlyPieces;
        ulong enemyPieces;
        ulong allPieces;
        int kingPos;
        int eKingPos;
        byte castlingRights;
        public ulong attackedSquares;
        public ulong pawnAttackedSquares;
        ulong legalSquares;
        ulong pinnedPieces;
        ulong emptySquares;
        ulong discoverySquares;

        //for quiesence
        ulong pawnChecks, knightChecks, bishopChecks, rookChecks, queenChecks;
        int fPawn, fKnight, fBishop, fRook, fQueen, fKing, ePawn, eKnight, eBishop, eRook, eQueen, eKing;
        Dictionary<int,ulong> pinnedMoves = new Dictionary<int, ulong>();
        Board board;
        public List<Move> GenerateMoves(Board boardState, bool capturesOnly){
            board = boardState;
            Init();

            GenerateAttackingData();
            if(doubleCheck){
                GenerateKingMoves(false);
                return moves;
            }
            bool quiesence = capturesOnly && !inCheck;

            InitQuiesence(quiesence);

            //print("pawns: " + pawnChecks + " || knights: " + knightChecks + " || bishops: " + bishopChecks + " || rooks: " + rookChecks + " || queens: "+ queenChecks);
            GenerateEnpassants();

            GeneratePawnMoves();

            GenerateKnightMoves();

            GenerateSlidingMoves(quiesence);
            
            GenerateKingMoves(quiesence);
            return moves;
        }
        public void Init(){
            moves = new List<Move>(64);
            inCheck = false;
            doubleCheck = false;

            friendlyPieces = 0uL;
            enemyPieces = 0uL;
            emptySquares = 0uL;

            allPieces = enemyPieces | friendlyPieces;
            
            attackedSquares = 0uL;
            pawnAttackedSquares = 0uL;

            whiteTurn = board.whiteTurn;

            fIndex = whiteTurn ? 0 : 1;
            eIndex = 1 - fIndex;
            weight = whiteTurn ? 1 : -1;

            fInd = whiteTurn ? 0 : 6;
            eInd = 6 - fInd;

            weight = whiteTurn ? 1 : -1;

            pieces = board.pieces;
            legalSquares = ~0uL;
            pinnedPieces = 0uL;
            pinnedMoves.Clear();
            fPawn = fInd; fKnight = fInd + 1; fBishop = fInd + 2; fRook = fInd + 3; fQueen = fInd + 4; fKing = fInd + 5; 
            ePawn = eInd; eKnight = eInd + 1; eBishop = eInd + 2; eRook = eInd + 3; eQueen = eInd + 4; eKing = eInd + 5;
            
            kingPos = Functions.FindBit(pieces[fKing]);
        }
        public void InitQuiesence(bool quiesence){
            eKingPos = Functions.FindBit(pieces[eKing]);
            discoverySquares = 0uL;
            if(quiesence){
                if(whiteTurn){
                    pawnChecks = ((1uL << eKingPos) >> 7) & ~(Precalculated.file[0]);
                    pawnChecks |= ((1uL << eKingPos) >> 9) & ~(Precalculated.file[7]);
                }else{
                    pawnChecks = ((1uL << eKingPos) << 7) & ~(Precalculated.file[7]);
                    pawnChecks |= ((1uL << eKingPos) << 9) & ~(Precalculated.file[0]);
                }

                knightChecks = Precalculated.knightMoves[eKingPos];
                bishopChecks = Precalculated.GetBishopAttacks(eKingPos, allPieces);
                rookChecks = Precalculated.GetRookAttacks(eKingPos, allPieces);
                queenChecks = Precalculated.GetQueenAttacks(eKingPos, allPieces);

                FindDiscoveries();
            }else{
                discoverySquares = ulong.MaxValue;
                pawnChecks = ulong.MaxValue;
                knightChecks = ulong.MaxValue;
                bishopChecks = ulong.MaxValue;
                rookChecks = ulong.MaxValue;
                queenChecks = ulong.MaxValue;
            }
        }
        public void FindDiscoveries(){
            //print("ekingpos: "+ eKingPos);
            for(int index = 0; index < 8; index++){
                //print("index: " + index);
                ulong intersect;
                if(index < 4){
                    intersect = Precalculated.directions[index][eKingPos] & (pieces[fRook] | pieces[fQueen]);
                    if(intersect == 0){
                        //print("no eyeing pieces");
                        continue;
                    }
                }
                else{
                    intersect = Precalculated.directions[index][eKingPos] & (pieces[fBishop] | pieces[fQueen]);
                    if(intersect == 0){
                        //print("no eyeing pieces");
                        continue;
                    }
                }

                int blockers = 0;
                ulong blockerPosition = 0uL;
                for(int i = 1; i <= Precalculated.DistanceToEdge[eKingPos][index]; i++){
                    ulong square = 1uL << (eKingPos + Precalculated.DirectionOffsets[index] * i);
                    //print("for square: " + square + " || blockers: " + blockers);

                    if(index < 4){
                        if((square & (pieces[fPawn] | pieces[fBishop] | pieces[fKnight])) != 0){
                            blockers++;
                            blockerPosition |= square;
                            if(blockers > 1){
                                //print("too many blockers");
                                break;
                            }
                        }
                        if((square & (pieces[fRook] | pieces[fQueen])) != 0){
                            if(blockers == 1){
                                discoverySquares |= blockerPosition;
                                break;
                            }else{
                                //print("not 1 piece in front");
                                break;
                            }
                        }
                    }
                    else{
                        if((square & (pieces[fPawn] | pieces[fRook] | pieces[fKnight])) != 0){
                            blockers++;
                            blockerPosition |= square;
                            if(blockers > 1){
                                //print("too many blockers");
                                break;
                            }
                        }
                        if((square & (pieces[fBishop] | pieces[fQueen])) != 0){
                            if(blockers == 1){
                                discoverySquares |= blockerPosition;
                                break;
                            }else{
                                //print("not 1 piece in front");
                                break;
                            }
                        }
                    }
                }

            }
            //print("discoverysquares: " + discoverySquares);
        }
        public void GenerateAttackingData() {
            //generate occupancy
            int startingIndex = whiteTurn ? 0 : 6;
            for(int i = startingIndex; i < startingIndex + 6; i++) {
                friendlyPieces |= pieces[i];
            }
            for (int i = 6-startingIndex; i < 12- startingIndex; i++) {
                enemyPieces |= pieces[i];
            }
            allPieces = friendlyPieces | enemyPieces;
            emptySquares = ~allPieces;

            GetAttackedSquares();
            ulong attackingPieces = 0;
            if(inCheck){
                legalSquares = 0;
                legalSquares |= Precalculated.pawnAttack[fIndex][kingPos] & pieces[eInd];
                attackingPieces|= Precalculated.pawnAttack[fIndex][kingPos] & pieces[eInd];
                
                legalSquares |= Precalculated.knightMoves[kingPos] & pieces[eKnight];
                attackingPieces|= Precalculated.knightMoves[kingPos] & pieces[eKnight];
            }

            for(int index = 0; index < 8; index ++)
            {
                ulong intersect;
                if(index < 4){
                    intersect = Precalculated.directions[index][kingPos] & (pieces[eRook] | pieces[eQueen]);
                    if(intersect == 0){
                        continue;
                    }
                }
                else{
                    intersect = Precalculated.directions[index][kingPos] & (pieces[eBishop] | pieces[eQueen]);
                    if(intersect == 0){
                        continue;
                    }
                }
                ulong ray = 0, attacker = 0;
                bool hasPins = true;
                for(int i = 1; i <= Precalculated.DistanceToEdge[kingPos][index]; i++){
                    ulong square = 1uL << (kingPos + Precalculated.DirectionOffsets[index] * i);
                    ray |= square;

                    if(index < 4){
                        if((square & (pieces[ePawn] | pieces[eBishop] | pieces[eKnight])) != 0){
                            hasPins = false;
                            break;
                        }
                    }
                    else{
                        if((square & (pieces[ePawn] | pieces[eRook] | pieces[eKnight])) != 0){
                            hasPins = false;
                            break;
                        }
                    }
                    if((square & intersect) != 0){
                        attacker = square & intersect;
                        break;
                    }
                }
                int blockers = Functions.BitCount(ray & friendlyPieces);

                if(blockers > 1 | !hasPins){
                    continue;
                }
                else if(blockers == 0){
                    if(attackingPieces != 0){
                        doubleCheck = true;
                        break;
                    }
                    attackingPieces |= attacker;
                    legalSquares |= ray;
                }
                else{
                    pinnedPieces |= ray & friendlyPieces;
                    pinnedMoves.Add(Functions.FindBit(ray & friendlyPieces), ray ^ (ray & friendlyPieces));
                }
            }
        }
        public void GetAttackedSquares(){
            ulong pawns = pieces[eInd]; ulong knight = pieces[eKnight]; ulong bishop = pieces[eBishop]; ulong rook = pieces[eRook]; ulong queen = pieces[eQueen];

            if(whiteTurn){
                attackedSquares |= (pawns >> 7 & ~Precalculated.file[0]) | (pawns >> 9 & ~Precalculated.file[7]);
            }
            else{
                attackedSquares |= (pawns << 7 & ~Precalculated.file[7]) | (pawns << 9 & ~Precalculated.file[0]);
            }
            pawnAttackedSquares = attackedSquares;

            ulong knights = 0;
            while(knight != 0){
                int i = Functions.pop_lsb(ref knight);
                attackedSquares |= Precalculated.knightMoves[i];
                knights |= Precalculated.knightMoves[i];
            }
            ulong bishops = 0;
            while(bishop != 0){
                int i = Functions.pop_lsb(ref bishop);
                attackedSquares |= Precalculated.GetBishopAttacks(i, allPieces ^ (1uL << kingPos));
                bishops |= Precalculated.GetBishopAttacks(i, allPieces ^ (1uL << kingPos));
            }
            ulong rooks = 0;
            while(rook != 0){
                int i = Functions.pop_lsb(ref rook);
                attackedSquares |= Precalculated.GetRookAttacks(i, allPieces ^ (1uL << kingPos));
                rooks |= Precalculated.GetRookAttacks(i, allPieces ^ (1uL << kingPos));
            }
            ulong queens = 0;
            while(queen != 0){
                int i = Functions.pop_lsb(ref queen);
                attackedSquares |= Precalculated.GetQueenAttacks(i, allPieces ^ (1uL << kingPos));
                queens |= Precalculated.GetQueenAttacks(i, allPieces ^ (1uL << kingPos));
            }

            attackedSquares |= Precalculated.kingMoves[Functions.FindBit(pieces[eKing])];
            if((attackedSquares & pieces[fKing]) != 0){
                inCheck = true;
            }
        }
        public void GeneratePawnMoves() {
            ulong u = pieces[fPawn] & ~pinnedPieces;
            ulong pinnedpawns = pieces[fPawn] & pinnedPieces;
            
            ulong dp; ulong pr; ulong p; ulong b; ulong left; ulong right; ulong p1; ulong p2; ulong p3;
            if(whiteTurn){
                pr = u & Precalculated.rank[6];
                p = u & ~pr;
                b = p << 8 & emptySquares & legalSquares & pawnChecks & (discoverySquares << 8);

                dp = (p << 8 & emptySquares & Precalculated.rank[2]) << 8 & emptySquares & legalSquares & pawnChecks & (discoverySquares << 16);

                left = p << 7 & ~Precalculated.file[7] & enemyPieces & legalSquares;
                right = p << 9 & ~Precalculated.file[0] & enemyPieces & legalSquares;

                p1 = pr << 8 & emptySquares & legalSquares;
                p2 = pr << 7 & ~Precalculated.file[7] & enemyPieces & legalSquares;
                p3 = pr << 9 & ~Precalculated.file[0] & enemyPieces & legalSquares;
            }
            else{
                pr = u & Precalculated.rank[1];
                p = u & ~pr;
                b = p >> 8 & emptySquares & legalSquares & pawnChecks & (discoverySquares >> 8);

                dp = (p >> 8 & emptySquares & Precalculated.rank[5]) >> 8 & emptySquares & legalSquares & pawnChecks & (discoverySquares >> 16);
                
                left = p >> 7 & ~Precalculated.file[0] & enemyPieces & legalSquares;
                right = p >> 9 & ~Precalculated.file[7] & enemyPieces & legalSquares;

                p1 = pr >> 8 & emptySquares & legalSquares;
                p2 = pr >> 7 & ~Precalculated.file[0] & enemyPieces & legalSquares;
                p3 = pr >> 9 & ~Precalculated.file[7] & enemyPieces & legalSquares;
            }
            while(b != 0){
                int i = Functions.pop_lsb(ref b);
                moves.Add(new Move(i - 8 * weight, i, fPawn));
            }
            while(dp != 0){
                int i = Functions.pop_lsb(ref dp);
                moves.Add(new Move(i - 16 * weight, i, fPawn, 7));
            }
            while(left != 0){
                int i = Functions.pop_lsb(ref left);
                moves.Add(new Move(i - 7 * weight,i, fPawn, 0, Functions.TypeAtSquare(board, i, eInd)));
            }
            while(right != 0){
                int i = Functions.pop_lsb(ref right);
                moves.Add(new Move(i - 9 * weight,i, fPawn, 0, Functions.TypeAtSquare(board, i, eInd)));
            }
            while(p1 != 0){
                int i = Functions.pop_lsb(ref p1);
                moves.Add(new Move(i - 8 * weight, i, fPawn, Move.FlagType.PromoteToKnight));
                moves.Add(new Move(i - 8 * weight, i, fPawn, Move.FlagType.PromoteToBishop));
                moves.Add(new Move(i - 8 * weight, i, fPawn, Move.FlagType.PromoteToRook));
                moves.Add(new Move(i - 8 * weight, i, fPawn, Move.FlagType.PromoteToQueen));  
            }
            while(p2 != 0){
                int i = Functions.pop_lsb(ref p2);
                moves.Add(new Move(i - 7 * weight, i, fPawn, Move.FlagType.PromoteToKnight, Functions.TypeAtSquare(board, i, eInd)));
                moves.Add(new Move(i - 7 * weight, i, fPawn, Move.FlagType.PromoteToBishop, Functions.TypeAtSquare(board, i, eInd)));
                moves.Add(new Move(i - 7 * weight, i, fPawn, Move.FlagType.PromoteToRook, Functions.TypeAtSquare(board, i, eInd)));
                moves.Add(new Move(i - 7 * weight, i, fPawn, Move.FlagType.PromoteToQueen, Functions.TypeAtSquare(board, i, eInd)));
            }
            while(p3 != 0){
                int i = Functions.pop_lsb(ref p3);
                moves.Add(new Move(i - 9 * weight, i, fPawn, Move.FlagType.PromoteToKnight, Functions.TypeAtSquare(board, i, eInd)));
                moves.Add(new Move(i - 9 * weight, i, fPawn, Move.FlagType.PromoteToBishop, Functions.TypeAtSquare(board, i, eInd)));
                moves.Add(new Move(i - 9 * weight, i, fPawn, Move.FlagType.PromoteToRook, Functions.TypeAtSquare(board, i, eInd)));
                moves.Add(new Move(i - 9 * weight, i, fPawn, Move.FlagType.PromoteToQueen, Functions.TypeAtSquare(board, i, eInd)));
            }
            while(pinnedpawns != 0){
                int sq = Functions.pop_lsb(ref pinnedpawns);
                ulong pawn = 1ul << sq;
                
                ulong capture1, capture2, push, doublep, pinRay = pinnedMoves[sq];
                bool promotion; 

                if(whiteTurn){
                    capture1 = pawn << 7 & ~Precalculated.file[7] & pinRay & enemyPieces & legalSquares; 
                    capture2 = pawn << 9 & ~Precalculated.file[0] & pinRay & enemyPieces & legalSquares;

                    push = pawn << 8 & pinRay & emptySquares & legalSquares & pawnChecks;
                    doublep = (pawn << 8 & Precalculated.rank[2] & emptySquares) << 8 & pinRay & emptySquares & legalSquares & pawnChecks & (discoverySquares << 16);

                    promotion = (pawn & Precalculated.rank[7]) != 0;
                }
                else{
                    capture1 = pawn >> 7 & ~Precalculated.file[0] & pinRay & enemyPieces & legalSquares;
                    capture2 = pawn >> 9 & ~Precalculated.file[7] & pinRay & enemyPieces & legalSquares;

                    push = pawn >> 8 & pinRay & emptySquares & legalSquares & pawnChecks;
                    doublep = (pawn >> 8 & Precalculated.rank[5] & emptySquares) >> 8 & pinRay & emptySquares & legalSquares & pawnChecks & (discoverySquares << 8);

                    promotion = (pawn & Precalculated.rank[0]) != 0;
                }

                if(promotion){
                    continue;
                }

                if(capture1 != 0){
                    moves.Add(new Move(sq, sq + 7 * weight, fPawn, 0, Functions.TypeAtSquare(board, sq + 7 * weight, eInd)));
                }
                if(capture2 != 0){
                    moves.Add(new Move(sq, sq + 9 * weight, fPawn, 0, Functions.TypeAtSquare(board, sq + 9 * weight, eInd)));
                }
                if(push != 0){
                    moves.Add(new Move(sq, sq + 8 * weight, fPawn));
                }
                if(doublep != 0){
                    moves.Add(new Move(sq, sq + 16 * weight, fPawn, 7));
                }
            }
        }
        public void GenerateKingMoves(bool capturesOnly) {
            if(capturesOnly){
                return;
            }
            ulong u = Precalculated.kingMoves[kingPos] & ~friendlyPieces & ~attackedSquares;
            ulong p = u & enemyPieces;
            while(p != 0){
                int to = Functions.pop_lsb(ref p);
                moves.Add(new Move(kingPos, to, fKing, 0, Functions.TypeAtSquare(board,to,eInd)));
            }
            ulong q = u & ~enemyPieces;
            while(q != 0){
                moves.Add(new Move(kingPos,Functions.pop_lsb(ref q),fKing));
            }

            if(!inCheck){
                int kingPosition = whiteTurn ? 4 : 60;
                if((Precalculated.castlingSquares[fIndex][0] & (attackedSquares | friendlyPieces)) == 0 && (~Board.castlingMasks[fIndex][0] & board.gameState) != 0){
                    moves.Add(new Move(kingPosition, kingPosition-2, fKing, 2));
                }
                if((Precalculated.castlingSquares[fIndex][1] & (attackedSquares | friendlyPieces)) == 0 && (~Board.castlingMasks[fIndex][1] & board.gameState) != 0){
                    moves.Add(new Move(kingPosition, kingPosition+2, fKing, 2));
                }
            }
        }
        public void GenerateEnpassants(){
            if(board.EnpassantFile() != 0){
                foreach(int x in Functions.BitPositions(pieces[fPawn] & ~pinnedPieces & Precalculated.EnpassantSquares[fIndex][board.EnpassantFile()-1]))
                {
                    moves.Add(new Move(x, 40-24*fIndex + board.EnpassantFile() - 1, fPawn, 1, 0));
                }
            }
        }
        public void GenerateKnightMoves() {
            ulong x = pieces[fKnight] & ~pinnedPieces;
 
            while(x!=0){
                int i = Functions.pop_lsb(ref x);
                ulong knightChecks2 = (1uL << i & discoverySquares) == 0? knightChecks : ulong.MaxValue;
                ulong u = Precalculated.knightMoves[i] & ~friendlyPieces & legalSquares;

                ulong c = u & enemyPieces;
                while(c != 0){
                    int s = Functions.pop_lsb(ref c);
                    moves.Add(new Move(i, s, fKnight, 0, Functions.TypeAtSquare(board,s,eInd)));    
                }

                ulong p = u & ~enemyPieces & knightChecks2;
                while(p != 0){
                    moves.Add(new Move(i, Functions.pop_lsb(ref p), fKnight)); 
                }
            }
        }
        public void GenerateSlidingMoves(bool capturesOnly) {
            ulong bishop = pieces[fBishop] & ~pinnedPieces;
            ulong rook = pieces[fRook] & ~pinnedPieces;
            ulong queen = pieces[fQueen] & ~pinnedPieces;

            ulong pinnedBishop = pieces[fBishop] & pinnedPieces;
            ulong pinnedRook = pieces[fRook] & pinnedPieces;            
            ulong pinnedQueen = pieces[fQueen] & pinnedPieces;

            while(bishop != 0){
                int s = Functions.pop_lsb(ref bishop);
                ulong u = Precalculated.GetBishopAttacks(s,allPieces) & ~friendlyPieces & legalSquares;

                ulong bishopChecks2 = (1uL << s & discoverySquares) == 0? bishopChecks : ulong.MaxValue;
                
                ulong a = u & enemyPieces;
                ulong p = u & ~enemyPieces & bishopChecks2;

                while(a != 0){
                    int square = Functions.pop_lsb(ref a);
                    moves.Add(new Move(s,square, fBishop, 0, Functions.TypeAtSquare(board,square,eInd)));
                }
                while(p != 0){
                    moves.Add(new Move(s,Functions.pop_lsb(ref p), fBishop));
                }
            }
            while(rook != 0){
                int s = Functions.pop_lsb(ref rook);
                ulong u = Precalculated.GetRookAttacks(s,allPieces) & ~friendlyPieces & legalSquares;

                ulong rookChecks2 = (1uL << s & discoverySquares) == 0? rookChecks : ulong.MaxValue;

                ulong a = u & enemyPieces;
                ulong p = u & ~enemyPieces & rookChecks2;

                while(a != 0){
                    int square = Functions.pop_lsb(ref a);
                    moves.Add(new Move(s,square, fRook, 0, Functions.TypeAtSquare(board,square,eInd)));
                }
                while(p != 0){
                    moves.Add(new Move(s,Functions.pop_lsb(ref p), fRook));
                }
            }
            while(queen != 0){
                int s = Functions.pop_lsb(ref queen);
                ulong u = Precalculated.GetQueenAttacks(s,allPieces) & ~friendlyPieces & legalSquares;  
                
                ulong a = u & enemyPieces;
                ulong p = u & ~enemyPieces & (rookChecks | bishopChecks);

                while(a != 0){
                    int square = Functions.pop_lsb(ref a);
                    moves.Add(new Move(s,square, fQueen, 0, Functions.TypeAtSquare(board,square,eInd)));
                }
                while(p != 0){
                    moves.Add(new Move(s,Functions.pop_lsb(ref p), fQueen));
                }
            }

            while(pinnedBishop != 0){
                int s = Functions.pop_lsb(ref pinnedBishop);
                ulong u = Precalculated.GetBishopAttacks(s,allPieces) & ~friendlyPieces & legalSquares & pinnedMoves[s];

                ulong bishopChecks2 = (1uL << s & discoverySquares) == 0? bishopChecks : ulong.MaxValue;

                ulong a = u & enemyPieces;
                ulong p = u & ~enemyPieces & bishopChecks2;

                while(a != 0){
                    int square = Functions.pop_lsb(ref a);
                    moves.Add(new Move(s,square, fBishop, 0, Functions.TypeAtSquare(board,square,eInd)));
                }
                while(p != 0){
                    moves.Add(new Move(s,Functions.pop_lsb(ref p), fBishop));
                }
            }
            while(pinnedRook != 0){
                int s = Functions.pop_lsb(ref pinnedRook);
                ulong u = Precalculated.GetRookAttacks(s,allPieces) & ~friendlyPieces & legalSquares & pinnedMoves[s];

                ulong rookChecks2 = (1uL << s & discoverySquares) == 0? rookChecks : ulong.MaxValue;

                ulong a = u & enemyPieces;
                ulong p = u & ~enemyPieces & rookChecks2;

                while(a != 0){
                    int square = Functions.pop_lsb(ref a);
                    moves.Add(new Move(s,square, fRook, 0, Functions.TypeAtSquare(board,square,eInd)));
                }
                while(p != 0){
                    moves.Add(new Move(s,Functions.pop_lsb(ref p), fRook));
                }
            }
            while(pinnedQueen != 0){
                int s = Functions.pop_lsb(ref pinnedQueen);
                ulong u = Precalculated.GetQueenAttacks(s,allPieces) & ~friendlyPieces & legalSquares & pinnedMoves[s];
                
                ulong a = u & enemyPieces;
                ulong p = u & ~enemyPieces & (rookChecks|bishopChecks);

                while(a != 0){
                    int square = Functions.pop_lsb(ref a);
                    moves.Add(new Move(s,square, fQueen, 0, Functions.TypeAtSquare(board,square,eInd)));
                }
                while(p != 0){
                    moves.Add(new Move(s,Functions.pop_lsb(ref p), fQueen));
                }
            }
        }

        public bool TerminalNode(Board boardstate){
            board = boardstate;
            Init();
            GenerateAttackingData();
            if(doubleCheck){
                ulong a = Precalculated.kingMoves[kingPos] & ~friendlyPieces & ~attackedSquares;
                if(a != 0){
                    return false;
                }
                return true;
            }
            #region Pawn
                ulong u = pieces[fPawn] & ~pinnedPieces;
                ulong pinnedpawns = pieces[fPawn] & pinnedPieces;
                ulong pr; ulong p; 
                if(whiteTurn){
                    pr = u & Precalculated.rank[6];
                    p = u & ~pr;

                    if((p << 8 & emptySquares & legalSquares) != 0 || 
                    ((p << 8 & emptySquares & Precalculated.rank[2]) << 8 & emptySquares & legalSquares) != 0 ||
                    (p << 7 & ~Precalculated.file[7] & enemyPieces & legalSquares) != 0 || 
                    (p << 9 & ~Precalculated.file[0] & enemyPieces & legalSquares) != 0 || 
                    (pr << 8 & emptySquares & legalSquares) != 0 || 
                    (pr << 7 & ~Precalculated.file[7] & enemyPieces & legalSquares) != 0 || 
                    (pr << 9 & ~Precalculated.file[0] & enemyPieces & legalSquares) != 0){
                        return false;
                    }
                }
                else{
                    pr = u & Precalculated.rank[1];
                    p = u & ~pr;

                    if((p >> 8 & emptySquares & legalSquares) != 0 || 
                    ((p >> 8 & emptySquares & Precalculated.rank[5]) >> 8 & emptySquares & legalSquares) != 0 ||
                    (p >> 7 & ~Precalculated.file[0] & enemyPieces & legalSquares) != 0 || 
                    (p >> 9 & ~Precalculated.file[7] & enemyPieces & legalSquares) != 0 || 
                    (pr >> 8 & emptySquares & legalSquares) != 0 || 
                    (pr >> 7 & ~Precalculated.file[0] & enemyPieces & legalSquares) != 0 || 
                    (pr >> 9 & ~Precalculated.file[7] & enemyPieces & legalSquares) != 0){
                        return false;
                    }
                }
            #endregion
            #region Knight
                ulong x = pieces[fKnight]& ~pinnedPieces;
                while(x != 0){
                    int i = Functions.pop_lsb(ref x);
                    u = Precalculated.knightMoves[i] & ~friendlyPieces & legalSquares;
                    if(u != 0){
                        return false;
                    }
                }
            #endregion
            #region Sliding
                ulong bishop = pieces[fBishop] & ~pinnedPieces;
                ulong bp1 = pieces[fBishop] & pinnedPieces;

                ulong rook = pieces[fRook] & ~pinnedPieces;
                ulong bp2 = pieces[fRook] & pinnedPieces;

                ulong queen = pieces[fQueen] & ~pinnedPieces;
                ulong bp3 = pieces[fQueen] & pinnedPieces;
                while(bishop != 0){
                    int s = Functions.pop_lsb(ref bishop);
                    if((Precalculated.GetBishopAttacks(s,allPieces) & ~friendlyPieces & legalSquares) != 0){
                        return false;
                    }
                }
                while(bp1 != 0){
                    int s = Functions.pop_lsb(ref bp1);
                    if((Precalculated.GetBishopAttacks(s,allPieces) & ~friendlyPieces & legalSquares & pinnedMoves[s]) != 0){
                        return false;
                    }
                }

                while(rook != 0){
                    int s = Functions.pop_lsb(ref rook);
                    if((Precalculated.GetRookAttacks(s,allPieces) & ~friendlyPieces & legalSquares) != 0){
                        return false;
                    }
                }
                while(bp2 != 0){
                    int s = Functions.pop_lsb(ref bp2);
                    if((Precalculated.GetRookAttacks(s,allPieces) & ~friendlyPieces & legalSquares & pinnedMoves[s]) != 0){
                        return false;
                    }
                }
                while(queen != 0){
                    int s = Functions.pop_lsb(ref queen);
                    if((Precalculated.GetQueenAttacks(s,allPieces) & ~friendlyPieces & legalSquares) != 0){
                        return false;
                    }
                }
                while(bp3 != 0){
                    int s = Functions.pop_lsb(ref bp3);
                    if((Precalculated.GetQueenAttacks(s,allPieces) & ~friendlyPieces & legalSquares & pinnedMoves[s]) != 0){
                        return false;
                    }
                }
            #endregion
            #region King
                u = Precalculated.kingMoves[kingPos] & ~friendlyPieces & ~attackedSquares;
                if(u != 0){
                    return false;
                }
            #endregion
            return true;
        }
    }
}