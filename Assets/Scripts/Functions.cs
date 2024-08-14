namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Numerics;
    public class Functions: MonoBehaviour
    {
        private const ulong DeBruijnSequence = 0x37E84A99DAE458F;
        private static readonly int[] MultiplyDeBruijnBitPosition =
        {
            0, 1, 17, 2, 18, 50, 3, 57,
            47, 19, 22, 51, 29, 4, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 5, 43, 34, 59, 8,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 7,
            62, 55, 45, 31, 13, 39, 36, 6,
            61, 44, 12, 35, 60, 11, 10, 9,
        };
        public static int[] log =
        {
            63,  0, 58,  1, 59, 47, 53,  2,
            60, 39, 48, 27, 54, 33, 42,  3,
            61, 51, 37, 40, 49, 18, 28, 20,
            55, 30, 34, 11, 43, 14, 22,  4,
            62, 57, 46, 52, 38, 26, 32, 41,
            50, 36, 17, 19, 29, 10, 13, 21,
            56, 45, 25, 31, 35, 16,  9, 12,
            44, 24, 15,  8, 23,  7,  6,  5
        };
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
        public static Dictionary<int, string> pieceTypes = new Dictionary<int,string>()
        {
            [0] = "W Pawn",
            [1] = "W Knight",
            [2] = "W Bishop",
            [3] = "W Rook",
            [4] = "W Queen",
            [5] = "W King",
            [6] = "B Pawn",
            [7] = "B Knight",
            [8] = "B Bishop",
            [9] = "B Rook",
            [10] = "B Queen",
            [11] = "B King"
        };
        
        public static int BitCount(ulong bitboard)
        {
            int i = 0;
            while (bitboard != 0)
            {
                i++;
                bitboard &= bitboard - 1;
            }
            return i;
        }
        public static int FindBit(ulong value)
        {
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return log[((value - (value >> 1)) * 0x07EDD5E59A4E28C2) >> 58];
        }
        public static List<int> BitPositions(ulong bits)
        {
            ulong temp = bits;
            List<int> bitPositions = new List<int>();
            while (temp != 0)
            {
                int pos = lsb(temp);
                bitPositions.Add(pos);
                temp &= temp - 1;
            }
            return bitPositions;
        }
        public static List<ulong> SplitUlong(ulong bits)
        {
            ulong temp = bits;
            List<ulong> output = new List<ulong>();
            while (temp != 0)
            {
                ulong pos = 1uL << lsb(temp);
                output.Add(pos);
                temp &= temp - 1;
            }
            return output;
        }

        public static void ClearBit(Board boardState, ulong clear, bool clearWhite)
        {
            if (clearWhite)
            {
                for (int i = 0; i < 6; i++)
                {
                    boardState.pieces[i] &= ~clear;
                }
            }
            else
            {
                for (int i = 6; i < 12; i++)
                {
                    boardState.pieces[i] &= ~clear;
                }
            }
        }
        public static void PrintBoard(Board board, string str)
        {
            print(str + " " +Display(board.pieces[0]) + " " + Display(board.pieces[1]) + " " + Display(board.pieces[2]) + " " + Display(board.pieces[3]) + " " + Display(board.pieces[4]) +
                " " + Display(board.pieces[5]) + " " + Display(board.pieces[6]) + " " + Display(board.pieces[7]) + " " + Display(board.pieces[8]) + " " + Display(board.pieces[9])
                + " " + Display(board.pieces[10]) + " " + Display(board.pieces[11]));
        }
        public static void PrintBoard(Board board)
        {
            print(Display(board.pieces[0]) + " " + Display(board.pieces[1]) + " " + Display(board.pieces[2]) + " " + Display(board.pieces[3]) + " " + Display(board.pieces[4]) +
                " " + Display(board.pieces[5]) + " " + Display(board.pieces[6]) + " " + Display(board.pieces[7]) + " " + Display(board.pieces[8]) + " " + Display(board.pieces[9])
                + " " + Display(board.pieces[10]) + " " + Display(board.pieces[11]));
        }

        public static ulong PopBit(ulong bitboard, int square)
        {
            return bitboard ^= 1uL << square;
        }
        public static int pop_lsb(ref ulong b)
        {
            int s = lsb(b);
            b &= b-1;
            return s;
        }
        public static int lsb(ulong bitboard)
        {
            return MultiplyDeBruijnBitPosition[((ulong)((long)bitboard & -(long)bitboard) * DeBruijnSequence) >> 58];
        }

        public static bool CompareBoards(Board board1, Board board2)
        {
            for (int i = 0; i < 12; i++)
            {
                if (board1.pieces[i] == board2.pieces[i])
                {
                    return true;
                }
            }
            return false;
        }

        public static ulong[] CreateSubArray(ulong[] array, int offset, int length)
	    {
            ulong[] result = new ulong[length];
            System.Array.Copy(array, offset, result, 0, length);
            return result;
        }
        public static ulong RandomUlong()
        {
            ulong u = 0uL;
            for (int i = 0; i <= 63; i++)
            {
                if (Random.value > 0.5f)
                {
                    u |= 1uL << i;
                }
            }
            return u;
        }
        public static long Display(ulong value)
        {
            if (value > long.MaxValue)
            {
                return (long)value - long.MaxValue - 1;
            }
            else return (long)value;
        }

        public static ulong SetNode(ulong bitboard, int position)
        {
            return bitboard | (1uL << position);
        }
        public static int TypeAtSquare(Board board, int square){
            for(int i = 0; i < 12;i++){
                if((1uL << square & board.pieces[i]) != 0){
                    return i;
                }
            }
            print("No piece on square");
            return -1;
        }
        public static int TypeAtSquare(Board board, ulong square){
            for(int i = 0; i < 12;i++){
                if((square & board.pieces[i]) != 0){
                    return i;
                }
            }
            print("No piece on square");
            return -1;            
        }

        public static int TypeAtSquare(Board board, int square, int friendlyIndex){
            for(int i = 0; i < 6;i++){
                if((1uL << square & board.pieces[i + friendlyIndex]) != 0){
                    return i;
                }
            }
            print("No piece on square");
            return -1;            
        }

        public static Board cloneBoard(Board board){
            Board board1  = new Board();
            board1.pieces = board.pieces;
            board1.whiteTurn = board.whiteTurn;
            return board1;
        }

        public static void QuickSort(List<Move> list, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(list, low, high);
                QuickSort(list, low, pivotIndex - 1);
                QuickSort(list, pivotIndex + 1, high);
            }
        }
        public static int Partition(List<Move> list, int low, int high)
        {
            // pivot (Element to be placed at right position)
            int pivot = list[high].Eval;
            int i = low - 1;

            for (int j = low; j <= high - 1; j++)
            {
                // If current element is smaller than the pivot
                if (list[j].Eval < pivot)
                {
                    i++;
                    //swap the selected one and the pivot
                    Move temporary = list[i];
                    list[i] = list[j];
                    list[j] = temporary;
                }
            }
            Move temp_ = list[i + 1];
            list[i + 1] = list[high];
            list[high] = temp_;
            return i + 1;
        }
        public static void QuickSort(List<(Move, int)> list, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(list, low, high);
                QuickSort(list, low, pivotIndex - 1);
                QuickSort(list, pivotIndex + 1, high);
            }
        }
        public static int Partition(List<(Move, int)> list, int low, int high)
        {
            // pivot (Element to be placed at right position)
            int pivot = list[high].Item2;
            int i = low - 1;

            for (int j = low; j <= high - 1; j++)
            {
                // If current element is smaller than the pivot
                if (list[j].Item2 < pivot)
                {
                    i++;
                    //swap the selected one and the pivot
                    (Move, int) temporary = list[i];
                    list[i] = list[j];
                    list[j] = temporary;
                }
            }
            (Move, int) temp_ = list[i + 1];
            list[i + 1] = list[high];
            list[high] = temp_;
            return i + 1;
        }

        public static void QuickSort(List<(int, int)> list, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(list, low, high);
                QuickSort(list, low, pivotIndex - 1);
                QuickSort(list, pivotIndex + 1, high);
            }
        }
        public static int Partition(List<(int, int)> list, int low, int high)
        {
            // pivot (Element to be placed at right position)
            int pivot = list[high].Item1;
            int i = low - 1;

            for (int j = low; j <= high - 1; j++)
            {
                // If current element is smaller than the pivot
                if (list[j].Item1 < pivot)
                {
                    i++;
                    //swap the selected one and the pivot
                    (int, int) temporary = list[i];
                    list[i] = list[j];
                    list[j] = temporary;
                }
            }

            (int, int) temp_ = list[i + 1];
            list[i + 1] = list[high];
            list[high] = temp_;
            return i + 1;
        }

        public static ulong HashPosition(Board board){
            ulong u = 0uL;
            if(!board.whiteTurn){
                u ^= Precalculated.blackToMoveZobrist;
            }
            for(int i= 0; i < 12; i++ ){
                ulong pieces = board.pieces[i];
                while(pieces != 0){
                    int s = Functions.pop_lsb(ref pieces);
                    u ^= Precalculated.bitstrings[i,s];
                }
            }
            return u;
        }

        public static ulong GenerateRandomBitstring(){
            ulong u = 0uL;
            for(int i = 0; i<64; i++){
                if(Random.Range(0,2) == 0){
                    u |= (1uL << i);
                }
            }
            return u;
        }
    }
}