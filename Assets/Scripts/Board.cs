namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class Board : MonoBehaviour
    {
        public ushort gameState;
        public ulong[] pieces = new ulong[12];
        public ulong friendlyPieces;
        public bool whiteTurn;        
        public Stack <ushort> GameStateArchive = new Stack<ushort>();
        public Stack <Move> moveArchive = new Stack<Move>();
        public Stack <ulong> ZobristArchive = new Stack<ulong>();

        public static ushort[][] castlingMasks = new ushort[][]{
            new ushort[] {0b1111111111111110, 0b1111111111111101},
            new ushort[] {0b1111111111110111, 0b1111111111111011}
        };

        public static ushort[][] PCastlingMasks = new ushort[][]{
            new ushort[] {0b1111111111101111, 0b1111111111011111},
            new ushort[] {0b1111111101111111, 0b1111111110111111}
        };

        public const ushort W_CastleMask = 0b1111111111111100;
        public const ushort B_CastleMask = 0b1111111111110011;
        public const ushort EPFileMask = 0b1111000011111111;
        //make move function
        public Board(Board board){
            for(int i = 0; i < 12; i++){
                pieces[i] = board.pieces[i];
            }
        }
        public Board(){

        }
        public int EnpassantFile()
        {
            return (gameState & ~EPFileMask) >> 8;
        }
        public int CastlingRights(){
            return(gameState & 0b0000000000001111);
        }
        public void MakeMove(Move move)
        {
            //Init variables

            GameStateArchive.Push(gameState);
            moveArchive.Push(move);
            gameState &= EPFileMask;
            if(move.isEmpty){
                print("empty move returned");
                return;
            }
            ulong zobristHash = 0uL;
            if(ZobristArchive.Count > 0){
            zobristHash= ZobristArchive.Peek() ^ Precalculated.blackToMoveZobrist;
            zobristHash ^= Precalculated.bitstrings[move.Type,move.StartSquare];
            }

            int fIndex = whiteTurn ? 0 : 6;
            int eIndex = 6-fIndex;
            int fInd = whiteTurn ? 0 : 1;
            int weight = whiteTurn ? 1 : -1;

            pieces[move.Type] ^= 1uL << move.StartSquare;

            if(move.Capture){
                pieces[move.CapturedType + eIndex] &= ~(1uL << move.TargetSquare);
                if(move.TargetSquare == Functions.FindBit(Precalculated.rookBeforeCastle[1-fInd][0])){
                    gameState &= castlingMasks[1-fInd][0];
                }
                if(move.TargetSquare == Functions.FindBit(Precalculated.rookBeforeCastle[1-fInd][1])){
                    gameState &= castlingMasks[1-fInd][1];
                }

                if(move.Flag != 1){
                    zobristHash ^= Precalculated.bitstrings[move.CapturedType + eIndex,move.TargetSquare];
                }
            } 

            pieces[move.Type] |= 1uL << move.TargetSquare;

            //CASTLING

            if(move.Type == 5 + fIndex){
                gameState &= whiteTurn ? W_CastleMask : B_CastleMask;
            }

            if(move.Type == 3 + fIndex){
                if(1uL << move.StartSquare == Precalculated.rookBeforeCastle[fInd][0] || 1uL << move.TargetSquare == Precalculated.rookBeforeCastle[fInd][0]){
                    gameState &= castlingMasks[fInd][0];
                }
                else if(1uL << move.StartSquare == Precalculated.rookBeforeCastle[fInd][1] || 1uL << move.TargetSquare== Precalculated.rookBeforeCastle[fInd][1]){
                    gameState &= castlingMasks[fInd][1];
                }
            }
            whiteTurn = !whiteTurn;
            switch(move.Flag){
                case 0: 
                    zobristHash ^= Precalculated.bitstrings[move.Type, move.TargetSquare];
                    break;
                case 7 : //double push
                    if(move.TargetSquare == move.StartSquare + 16 || move.TargetSquare == move.StartSquare - 16){
                        gameState |= (ushort)((1 + move.StartSquare % 8) << 8);
                    }
                    zobristHash ^= Precalculated.bitstrings[move.Type, move.TargetSquare];
                    break;
                case 2: //castling
                    zobristHash ^= Precalculated.bitstrings[move.Type, move.TargetSquare];

                    if(move.TargetSquare < move.StartSquare){//queen side
                        pieces[fIndex + 3] &= ~Precalculated.rookBeforeCastle[fInd][0];
                        pieces[fIndex + 3] |= Precalculated.rookAfterCastle[fInd][0];
                        zobristHash ^= Precalculated.bitstrings[fIndex + 3, Functions.FindBit(Precalculated.rookBeforeCastle[fInd][0])];
                        zobristHash ^= Precalculated.bitstrings[fIndex + 3, Functions.FindBit(Precalculated.rookAfterCastle[fInd][0])];
                    }
                    else{//king side
                        pieces[fIndex + 3] &= ~Precalculated.rookBeforeCastle[fInd][1];
                        pieces[fIndex + 3] |= Precalculated.rookAfterCastle[fInd][1];
                        zobristHash ^= Precalculated.bitstrings[fIndex + 3, Functions.FindBit(Precalculated.rookAfterCastle[fInd][1])];
                        zobristHash ^= Precalculated.bitstrings[fIndex + 3, Functions.FindBit(Precalculated.rookAfterCastle[fInd][1])];
                    }
                    break;
                case 1: //enpassant
                    zobristHash ^= Precalculated.bitstrings[move.Type, move.TargetSquare];
                    zobristHash ^= Precalculated.bitstrings[6-fIndex, move.TargetSquare + 8 * -weight];
                    pieces[6-fIndex] ^= 1uL << (move.TargetSquare + 8 * -weight);
                    break;
                case 3 : case 4 : case 5 : case 6 :
                    zobristHash ^= Precalculated.bitstrings[fIndex+move.Flag-2, move.TargetSquare];
                    pieces[fIndex] ^= 1uL << move.TargetSquare;
                    pieces[fIndex + move.Flag - 2] |=  1uL << move.TargetSquare;
                    break;
            }

            ZobristArchive.Push(zobristHash);
            return;
        }
        public void UnmakeMove()
        {
            int fIndex = whiteTurn ? 0 : 6;
            int eIndex = 6-fIndex;
            int fInd = whiteTurn ? 0 : 1;
            int weight = whiteTurn ? 1 : -1;

            ZobristArchive.Pop();
            gameState = GameStateArchive.Peek();
            GameStateArchive.Pop();
            Move move = moveArchive.Peek();
            moveArchive.Pop();

            pieces[move.Type] |= 1uL << move.StartSquare;
            pieces[move.Type] &= ~(1uL << move.TargetSquare);  

            switch(move.Flag){
                case 0:
                    break;
                case 1:
                    //enpassant
                    pieces[fIndex] |= 1uL << move.TargetSquare + 8 * weight;
                    break;
                case 2: 
                    //castling
                    if(move.TargetSquare > move.StartSquare){
                        //king side
                        pieces[eIndex + 3] ^= Precalculated.rookBeforeCastle[1-fInd][1];
                        pieces[eIndex + 3] ^= Precalculated.rookAfterCastle[1-fInd][1];
                    }
                    else{
                        //queen side
                        pieces[eIndex + 3] ^= Precalculated.rookBeforeCastle[1-fInd][0];
                        pieces[eIndex + 3] ^= Precalculated.rookAfterCastle[1-fInd][0];
                    }
                    break;
                case 3: case 4 : case 5: case 6:
                    pieces[move.Flag + eIndex - 2] &= ~(1uL << move.TargetSquare);
                    //promotion
                    break;
            }

            if(move.Capture && move.Flag != 1){
                pieces[move.CapturedType + fIndex] |= 1uL << move.TargetSquare;
            }
            whiteTurn = !whiteTurn;
        }
    }
}