namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public static class Precalculated
    {
        // 0 - queen side castling
        // 1 - king side castling
        public static ulong blackToMoveZobrist;
        public static ulong[,] bitstrings = new ulong[12,64];

        public static ulong[][] castlingSquares = new ulong[][]{
            new ulong[] {1uL << 1 | 1uL << 2 | 1uL << 3, 1uL << 5 | 1uL << 6},
            new ulong[] {1uL << 57 | 1uL << 58 | 1uL << 59, 1uL << 61 | 1uL << 62}
        };
        public static ulong[][] rookBeforeCastle = new ulong[][]{
            new ulong[] {1uL, 1uL << 7},
            new ulong[] {1uL << 56, 1uL << 63}
        };
        public static ulong[][] rookAfterCastle = new ulong[][]{
            new ulong[] {1uL << 3, 1uL << 5},
            new ulong[] {1uL << 59, 1uL << 61}
        };

        public static ulong[] knightMoves = new ulong[64];
        public static ulong[] kingMoves = new ulong[64];

        public static ulong[][] singlePush = new ulong[2][];
        public static ulong[][] doublePush = new ulong[2][];
        public static ulong[][] pawnAttack = new ulong[2][];

        public static ulong[][] EnpassantSquares = new ulong[2][];

        public static ulong[] file = new ulong[8];
        public static ulong[] rank = new ulong[8];

        public static int[][] DistanceToEdge = new int[64][];
        public static readonly int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };

        public static ulong[] bishopMasks = new ulong[64];
        public static ulong[] rookMasks = new ulong[64];
        public static ulong[][] bishopAttacks = new ulong[64][]; //512 max occupancies for each square//
        public static ulong[][] rookAttacks = new ulong[64][]; //4096 max occupancies for each square//
        public static ulong[][] bishopAttackedSquares = new ulong[64][];
        public static ulong[][] rookAttackedSquares = new ulong[64][];
        public static string[] squareNames =
        {
            "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
            "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
            "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8"
        };
        public static ulong[] rookMagics =
        {
            0xa8002c000108020UL, 0x6c00049b0002001UL, 0x100200010090040UL, 0x2480041000800801UL, 0x280028004000800UL, 0x900410008040022UL, 0x280020001001080UL, 0x2880002041000080UL,
            0xa000800080400034UL, 0x4808020004000UL, 0x2290802004801000UL, 0x411000d00100020UL, 0x402800800040080UL, 0xb000401004208UL, 0x2409000100040200UL, 0x1002100004082UL,
            0x22878001e24000UL, 0x1090810021004010UL, 0x801030040200012UL, 0x500808008001000UL, 0xa08018014000880UL, 0x8000808004000200UL, 0x201008080010200UL, 0x801020000441091UL,
            0x800080204005UL, 0x1040200040100048UL, 0x120200402082UL, 0xd14880480100080UL, 0x12040280080080UL, 0x100040080020080UL, 0x9020010080800200UL, 0x813241200148449UL,
            0x491604001800080UL, 0x100401000402001UL, 0x4820010021001040UL, 0x400402202000812UL, 0x209009005000802UL, 0x810800601800400UL, 0x4301083214000150UL, 0x204026458e001401UL,
            0x40204000808000UL, 0x8001008040010020UL, 0x8410820820420010UL, 0x1003001000090020UL, 0x804040008008080UL, 0x12000810020004UL, 0x1000100200040208UL, 0x430000a044020001UL,
            0x280009023410300UL, 0xe0100040002240UL, 0x200100401700UL, 0x2244100408008080UL, 0x8000400801980UL, 0x2000810040200UL, 0x8010100228810400UL, 0x2000009044210200UL,
            0x4080008040102101UL, 0x40002080411d01UL, 0x2005524060000901UL, 0x502001008400422UL, 0x489a000810200402UL, 0x1004400080a13UL, 0x4000011008020084UL, 0x26002114058042UL
        };
        public static ulong[] bishopMagics = {
            0x89a1121896040240UL,0x2004844802002010UL,0x2068080051921000UL,0x62880a0220200808UL,0x4042004000000UL,0x100822020200011UL,0xc00444222012000aUL,0x28808801216001UL,
            0x400492088408100UL,0x201c401040c0084UL,0x840800910a0010UL,0x82080240060UL,0x2000840504006000UL,0x30010c4108405004UL,0x1008005410080802UL,0x8144042209100900UL,
            0x208081020014400UL,0x4800201208ca00UL,0xf18140408012008UL,0x1004002802102001UL,0x841000820080811UL,0x40200200a42008UL,0x800054042000UL,0x88010400410c9000UL,
            0x520040470104290UL,0x1004040051500081UL,0x2002081833080021UL,0x400c00c010142UL,0x941408200c002000UL,0x658810000806011UL,0x188071040440a00UL,0x4800404002011c00UL,
            0x104442040404200UL,0x511080202091021UL,0x4022401120400UL,0x80c0040400080120UL,0x8040010040820802UL,0x480810700020090UL,0x102008e00040242UL,0x809005202050100UL,
            0x8002024220104080UL,0x431008804142000UL,0x19001802081400UL,0x200014208040080UL,0x3308082008200100UL,0x41010500040c020UL,0x4012020c04210308UL,0x208220a202004080UL,
            0x111040120082000UL,0x6803040141280a00UL,0x2101004202410000UL,0x8200000041108022UL,0x21082088000UL,0x2410204010040UL,0x40100400809000UL,0x822088220820214UL,
            0x40808090012004UL,0x910224040218c9UL,0x402814422015008UL,0x90014004842410UL,0x1000042304105UL,0x10008830412a00UL,0x2520081090008908UL,0x40102000a0a60140UL,
    };
        public static int[] bishopRelevantBits =
        {
            6, 5, 5, 5, 5, 5, 5, 6,
            5, 5, 5, 5, 5, 5, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 5, 5, 5, 5, 5, 5,
            6, 5, 5, 5, 5, 5, 5, 6
        };
        public static int[] rookRelevantBits =
        {
            12, 11, 11, 11, 11, 11, 11, 12,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            12, 11, 11, 11, 11, 11, 11, 12
        };
        public static ulong[][] directions = new ulong[8][];
        public static ulong[] wSquareAhead = new ulong[64];
        public static ulong[] bSquareAhead = new ulong[64];

        #region PieceSquareTable
        public static int[][] PSQT = {
            new int[] {//wpawn
                0,  0,  0,  0,  0,  0,  0,  0,
                50, 50, 50, 50, 50, 50, 50, 50,
                10, 10, 20, 30, 30, 20, 10, 10,
                5,  5, 10, 25, 25, 10,  5,  5,
                0,  0,  0, 25, 20,  0,  0,  0,
                3, -5,-10,  0,  0,-10, -5,  3,
                5, 10, 10,-20,-20, 10, 10,  5,
                0,  0,  0,  0,  0,  0,  0,  0
            },
            new int[]{//wknight
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -30,  0, 14, 16, 16, 14,  0,-30,
                -30,  5, 16, 23, 23, 16,  5,-30,
                -35,  0, 15, 20, 20, 15,  0,-35,
                -30,  5, 14, 16, 16, 15,  5,-30,
                -40,-18,  0,  5,  5,  0,-18,-40,
                -45,-20,-18,-15,-15,-18,-20,-45
            },
            new int[]{//wbishop
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -5,   0,  5, 10, 10,  5,  0, -5,
                -5,   7,  5, 12, 12,  5,  7, -5,
                -4,   0, 12, 10, 10, 12,  0, -4,
                -5,  10, 10, 7, 7, 10, 10, -5,
                -5,  12,  0,  0,  0,  0, 12, -5,
                -10,-12,-10,-10,-10,-10,-12,-10
            },
            new int[]{//wrook
                0,  0,  0,  5,  5,  0,  0,  0,
                15, 20, 20, 30, 30, 20, 20,  15,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  10,  10,  0,  0, -5,
                0,  0,  5,  10,  10,  5,  0,  0
            },
            new int[]{//wqueen
                -20,-10,-10, -5, -5,-10,-10,-20,
                -10,  0,  0,  2,  0,  0,  0,-10,
                -10,  3,  5,  6,  5,  0,  0,-10,
                -5,  2,  5,  6,  6,  5,  0, -5,
                0 ,  3,  5,  7,  7,  5,  3, 0,
                -10,  4,  5,  6,  5,  4,  0,-10,
                -10,  0,  4,  5,  4,  0,  0,-10,
                -20,-10,-10,  0,  0,-10,-10,-20
            },
            new int[]{//wking
                -30,-40,-40,-50,-50,-40,-40,-30,
                -40,-60,-60,-60,-60,-60,-60,-40,
                -40,-60,-60,-60,-60,-60,-60,-40,
                -40,-50,-55,-60,-60,-55,-50,-40,
                -40,-45,-50,-54,-54,-50,-45,-40,
                -10,-20,-22,-24,-24,-22,-20,-10,
                10, 30, -3, -5, -5, -3, 30, 10,
                20, 40, 35,  0,  0, 20, 50, 20
            },
            new int[]{//bpawn
                0,  0,  0,  0,  0,  0,  0,  0,
                5, 10, 10,-20,-20, 10, 10,  5,
                3, -5,-10,  0,  0,-10, -5,  3,               
                0,  0,  0, 20, 20,  0,  0,  0,
                5,  5, 10, 25, 25, 10,  5,  5,
                15, 10, 20, 30, 30, 20, 10, 15,
                50, 50, 50, 50, 50, 50, 50, 50,
                0,  0,  0,  0,  0,  0,  0,  0
            },
            new int[]{//bknight
                -45,-20,-18,-15,-15,-18,-20,-45,
                -40,-18,  0,  5,  5,  0,-18,-40,
                -30,  5, 14, 16, 16, 15,  5,-30,
                -35,  0, 15, 20, 20, 15,  0,-35,
                -30,  5, 16, 23, 23, 16,  5,-30,
                -30,  0, 14, 16, 16, 14,  0,-30,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50
            },
            new int[]{//bbishop
                -10,-12,-10,-10,-10,-10,-12,-10,
                -5,  12,  0,  0,  0,  0, 12, -5,
                -5,  10, 10,  7,  7, 10, 10, -5,
                -4,   0, 12, 10, 10, 12,  0, -4,
                -5,   7,  5, 12, 12,  5,  7, -5,
                -5,   0,  5, 10, 10,  5,  0, -5,
                -10, 0,   0,  0,  0,  0,  0,-10,
                -20,-10,-10,-10,-10,-10,-10,-20
            },
            new int[]{//brook
                0,  0,  5,  10,  10,  5,  0,  0,
                -5, 0,  0,  10,  10,  0,  0, -5,
                -5, 0,  0,   0,   0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                10, 20, 20, 30, 30, 20, 20, 10,
                0,  0,  0,  5,  5,  0,  0,  0
            },
            new int[]{//bqueen
                -20,-10,-10,  0,  0,-10,-10,-20,
                -10,  0,  4,  5,  4,  0,  0,-10,
                -10,  4,  5,  6,  5,  4,  0,-10,
                0 ,  3,  5,  7,  7,  5,  3, 0,
                -5,  2,  5,  6,  6,  5,  0, -5,
                -10,  3,  5,  6,  5,  0,  0,-10,
                -10,  0,  0,  2,  0,  0,  0,-10,
                -20,-10,-10, -5, -5,-10,-10,-20
            },
            new int[]{//bking
                20, 40, 35,  0,  0, 20, 50, 20,
                10, 30, -3, -5, -5, -3, 30, 10,
                -10,-20,-22,-24,-24,-22,-20,-10,
                -40,-45,-50,-54,-54,-50,-45,-40,
                -40,-60,-60,-60,-60,-60,-60,-40,
                -40,-60,-60,-60,-60,-60,-60,-40,
                -40,-60,-60,-60,-60,-60,-60,-40,
                -30,-40,-40,-50,-50,-40,-40,-30
            }
        };
        public static int[] EG_WKingTable =
        {
            -75,-40,-30,-20,-20,-30,-40,-75,
            -30,-15,-10,  0,  0,-10,-15,-30,
            -30,-5, 20, 30, 30, 20,-5,-30,
            -30,-5, 30, 40, 40, 30,-5,-30,
            -30,-5, 30, 40, 40, 30,-5,-30,
            -30,-5, 20, 30, 30, 20,-5,-30,
            -30,-20,-10,-10,-10,-10,-20,-30,
            -75,-30,-30,-30,-30,-30,-30,-75
        };    
        public static int[] EG_BKingTable =
        {
            -75,-30,-30,-20,-20,-30,-30,-75,
            -30,-20,-10,-10,-10,-10,-20,-30,
            -30,-5, 20, 30, 30, 20,-5,-30,
            -30,-5, 30, 40, 40, 30,-5,-30,
            -30,-5, 30, 40, 40, 30,-5,-30,
            -30,-5, 20, 30, 30, 20,-5,-30,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -75,-40,-30,-30,-30,-30,-40,-75
        };
        
    #endregion
        public static void InitPrecalculated()
        {
            DistancesToEdge();
            SetDirections();
            FileRanks();
            SetAhead();
            KingMoves();
            KnightMoves();
            PawnMoves();
            SetEnpassant();
            SetMasks();
            InitializeMagics();
            SetPSQTs();
            InitZobristHashing();
        }
        private static void SetAhead(){
            for(int i = 0; i< 8;i++){
                for(int j = 0; j< 8;j++){
                    for(int k = i + 1; k < 8; k++){
                        wSquareAhead[8*i + j] |= rank[k];
                    }

                    for(int k = i -1; k >= 0; k--){
                        bSquareAhead[8*i + j] |= rank[k];
                    }
                }
            } 
        }
        private static void FileRanks()
        {
            for(int i = 0; i < 64; i++)
            {
                file[i % 8] |= 1uL << i;
                rank[i / 8] |= 1uL << i;
            }
        }
        private static void PawnMoves()
        {
            for(int a = 0; a < 2;a ++)
            {
                // 0  white 1 black
                singlePush[a] = new ulong[64];
                doublePush[a] = new ulong[64]; 
                pawnAttack[a] = new ulong[64];
                for(int i = 0; i < 64;i++)
                {
                    singlePush[a][i] |= a == 0 ? 1uL << i + 8 & ~rank[0] : 1uL << i- 8 & ~rank[7];
                    //If white, shift up 8, if black, shift down 8 and exclude irregularities when on last rank
                    doublePush[a][i] |= a == 0 ? 1uL << i + 16 & rank[3] : 1uL << i - 16 & rank[4];
                    //If pawn is on starting square, have double push option
                    pawnAttack[a][i] |= a == 0 ? 1uL << i + 7 & ~file[7] & ~rank[0] : 1uL << i - 7 & ~file[0] & ~rank[7];
                    pawnAttack[a][i] |= a == 0 ? 1uL << i + 9 & ~file[0] & ~rank[0] : 1uL << i - 9 & ~file[7] & ~rank[7];
                }
            }
        }
        private static void SetEnpassant(){
            EnpassantSquares[0] = new ulong[8];
            for(int i = 0; i < 8; i++){
                EnpassantSquares[0][i] |= 1uL << 32 + i + 1 & ~file[0];
                EnpassantSquares[0][i] |= 1uL << 32 + i - 1 & ~file[7];
            }
            EnpassantSquares[1] = new ulong[8];
            for(int i = 0; i < 8; i++){
                EnpassantSquares[1][i] |= 1uL << 24 + i + 1 & ~file[0];
                EnpassantSquares[1][i] |= 1uL << 24 + i - 1 & ~file[7];
            }

        }
        private static void KingMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                kingMoves[i] |= 1uL << i + 8 & ~rank[0];
                kingMoves[i] |= 1uL << i - 8 & ~rank[7];
                kingMoves[i] |= 1uL << i + 1 & ~file[0];
                kingMoves[i] |= 1uL << i - 1 & ~file[7];

                kingMoves[i] |= 1uL << i + 7 & ~file[7] & ~rank[0];
                kingMoves[i] |= 1uL << i - 7 & ~file[0] & ~rank[7];
                kingMoves[i] |= 1uL << i + 9 & ~file[0] & ~rank[0];
                kingMoves[i] |= 1uL << i - 9 & ~file[7] & ~rank[7];
            }
        }
        public static void KnightMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                knightMoves[i] |= 1uL << i + 17 & ~file[0] & ~rank[0] & ~rank[1];
                knightMoves[i] |= 1uL << i + 15 & ~file[7] & ~rank[0] & ~rank[1];
                knightMoves[i] |= 1uL << i + 6 & ~file[7] & ~file[6] & ~rank[0];
                knightMoves[i] |= 1uL << i + 10 & ~file[0] & ~file[1] & ~rank[0];

                knightMoves[i] |= 1uL << i - 17 & ~file[7] & ~rank[7] & ~rank[6];
                knightMoves[i] |= 1uL << i - 15 & ~file[0] & ~rank[7] & ~rank[6];
                knightMoves[i] |= 1uL << i - 6 & ~file[0] & ~file[1] & ~rank[7];
                knightMoves[i] |= 1uL << i - 10 & ~file[7] & ~file[6] & ~rank[7];
            }
        }
        public static void SetMasks()
        {
            for (int square = 0; square < 64; square++)
            {
                ulong rookSquares = 0uL;
                for (int dIndex = 0; dIndex < 4; dIndex++)
                {
                    for (int offset = 1; offset < DistanceToEdge[square][dIndex]; offset++)
                    {
                        rookSquares |= 1uL << (square + DirectionOffsets[dIndex] * offset);
                    }
                }
                rookMasks[square] = rookSquares;

                // bishop moves \\

                ulong bishopSquares = 0uL;
                for (int dIndex = 4; dIndex < 8; dIndex++)
                {
                    for (int offset = 1; offset < DistanceToEdge[square][dIndex]; offset++)
                    {
                        bishopSquares |= 1uL << (square + DirectionOffsets[dIndex] * offset);
                    }
                }
                bishopMasks[square] = bishopSquares;
            }
        }
        public static ulong GetBishopAttacks(int square, ulong occupancy)
        {
            occupancy &= bishopMasks[square];
            occupancy *= bishopMagics[square];
            occupancy >>= 64 - bishopRelevantBits[square];
            return bishopAttacks[square][occupancy];
        }
        public static ulong GetRookAttacks(int square, ulong occupancy)
        {
            occupancy &= rookMasks[square];
            occupancy *= rookMagics[square];
            occupancy >>= 64 - rookRelevantBits[square];
            return rookAttacks[square][occupancy];
        }
        public static ulong GetQueenAttacks(int square, ulong occupancy)
        {
            ulong rookOccupancy = occupancy;
            //double for maniupation
            occupancy &= bishopMasks[square];
            occupancy *= bishopMagics[square];
            occupancy >>= 64 - bishopRelevantBits[square];
            ulong queenAttacks = bishopAttacks[square][occupancy];

            rookOccupancy &= rookMasks[square];
            rookOccupancy *= rookMagics[square];
            rookOccupancy >>= 64 - rookRelevantBits[square];
            queenAttacks |= rookAttacks[square][rookOccupancy];
            return queenAttacks;
        }
        public static void InitializeMagics()
        {
            for (int square = 0; square < 64; square++)
            {
                bishopAttacks[square] = new ulong[512];
                rookAttacks[square] = new ulong[4096];

                ulong _rookMask = rookMasks[square];
                ulong _bishopMask = bishopMasks[square];

                int rookBitCount = Functions.BitCount(_rookMask);
                int bishopBitCount = Functions.BitCount(_bishopMask);

                int rookOccCount = (int)1uL << rookBitCount;
                int bishopOccCount = (int)1uL << bishopBitCount;

                //bishop
                for (int count = 0; count < bishopOccCount; count++)
                {
                    ulong bishopOccupancy = SetOccupancy(count, bishopBitCount, _bishopMask);
                    ulong bishopIndex = (bishopOccupancy * bishopMagics[square]) >> (64 - bishopRelevantBits[square]);
                    bishopAttacks[square][bishopIndex] = BishopMoves(square, bishopOccupancy);
                }

                //rook
                for (int count = 0; count < rookOccCount; count++)
                {
                    ulong rookOccupancy = SetOccupancy(count, rookBitCount, _rookMask);
                    ulong rookIndex = (rookOccupancy * rookMagics[square]) >> (64 - rookRelevantBits[square]);
                    rookAttacks[square][rookIndex] = RookMoves(square, rookOccupancy);
                }
            }
        }
        public static ulong SetOccupancy(int index, int bitCount, ulong attackMask)
        {
            ulong occupancy = 0uL;
            for (int count = 0; count < bitCount; count++)
            {
                int square = Functions.lsb(attackMask);
                attackMask = Functions.PopBit(attackMask, square);
                if ((index & (1 << count)) != 0)
                {
                    occupancy |= 1uL << square;
                }
            }
            return occupancy;
        }
        public static ulong RookMoves(int square, ulong blockers)
        {
            ulong rookSquares = 0uL;
            for (int dIndex = 0; dIndex < 4; dIndex++)
            {
                for (int offset = 0; offset < DistanceToEdge[square][dIndex]; offset++)
                {
                    rookSquares = Functions.SetNode(rookSquares, square + DirectionOffsets[dIndex] * (offset + 1));
                    if ((Functions.SetNode(0uL, square + DirectionOffsets[dIndex] * (offset + 1)) & blockers) != 0)
                    {
                        break;
                    }
                }
            }
            return rookSquares;
        }
        public static ulong BishopMoves(int square, ulong blockers)
        {
            ulong rookSquares = 0uL;
            for (int dIndex = 4; dIndex < 8; dIndex++)
            {
                for (int offset = 0; offset < DistanceToEdge[square][dIndex]; offset++)
                {
                    rookSquares = Functions.SetNode(rookSquares, square + DirectionOffsets[dIndex] * (offset + 1));
                    if ((Functions.SetNode(0uL, square + DirectionOffsets[dIndex] * (offset + 1)) & blockers) != 0)
                    {
                        break;
                    }
                }
            }
            return rookSquares;
        }
        public static void SetDirections(){
            for(int index = 0; index < 8; index++)
            {
                directions[index] = new ulong[64];
                for(int square = 0; square < 64; square++)
                {
                    ulong u = 0uL;
                    for(int x = 1; x <= DistanceToEdge[square][index]; x ++){
                        u |= 1uL << (square + DirectionOffsets[index] * x);
                    }
                    directions[index][square] = u;
                }
            }
        }
        public static void DistancesToEdge()
        {
            for(int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {
                    DistanceToEdge[x + y * 8] = new int[8] 
                    {
                        7 - y, y, x, 7-x, Mathf.Min(7-y, x), Mathf.Min(y, 7-x), Mathf.Min(7-y, 7-x), Mathf.Min(y,x)
                    };
                }
            }
        }
        public static void SetPSQTs(){
            for(int i = 0; i < 12; i++){
                PSQT[i] = FlipArray(PSQT[i]);
            }
        }
        public static int[] FlipArray(int[] array)
        {
            int[] tempArray = new int[array.Length];
            int iterations = 0;
            for(int y = 7; y >= 0; y--)
            {
                for(int x = 0; x < 8; x++)
                {
                    tempArray[iterations] = array[y * 8 + x];
                    iterations++;
                }
            }
            return tempArray;
        }
        public static void InitZobristHashing(){
            for(int i = 0; i< 64; i++){
                for(int j = 0; j < 12; j++){
                    bitstrings[j,i] = Functions.GenerateRandomBitstring();
                }
            }
            blackToMoveZobrist = Functions.GenerateRandomBitstring();
        }
    }
}