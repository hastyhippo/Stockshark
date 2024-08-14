namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GameManager : MonoBehaviour
    {
        public static Board currentBoard;
        public static List<Move> possibleMoves;
        public MoveGenerator moveGen;
        public Evaluation evaluation;
        public ChessAI chessAI;
        public FEN fenScript;
        public Search search;
        public static int moveCount = 1;    
        public static int maxValue = 2147483647;
        public static int minValue = -2147483647;   

        public int GameType;
        //1 is computer vs computer
        //2 is player (white) vs computer
        //3 is player (black) vs computer
        //4 is player vs player 
        public float wait;
        private void Awake()
        {
            Precalculated.InitPrecalculated();
        }
        void Start()
        {
            UpdateCurrentBoard(true);

            if(possibleMoves.Count == 0){
                ChessAI.gameOver = true;
                if(moveGen.inCheck){
                    if(currentBoard.whiteTurn){
                        print("Black wins");
                    }
                    else{
                        print("White wins");
                    }
                }
                else{
                    print("Draw");
                }
            }
        }
        void Update(){
            if(GameType == 0){
                if(!ChessAI.gameOver){
                    currentBoard.MakeMove(chessAI.Negamax(chessAI.Depth, currentBoard));
                    UpdateCurrentBoard(false);
                    moveCount ++;
                }
                if(Input.GetKeyDown(KeyCode.R)){
                    ChessAI.gameOver = false;
                    UpdateCurrentBoard(true);
                }
            }

            if(GameType == 1){
                if(currentBoard.whiteTurn == false){
                    if(!ChessAI.gameOver){
                        currentBoard.MakeMove(chessAI.Negamax(chessAI.Depth, currentBoard));
                        UpdateCurrentBoard(false);
                        moveCount ++;
                    }
                }
            }
            if(GameType == 2){
                if(currentBoard.whiteTurn == true){
                    if(!ChessAI.gameOver){
                        currentBoard.MakeMove(chessAI.Negamax(chessAI.Depth, currentBoard));
                        UpdateCurrentBoard(false);
                        moveCount ++;
                    } 
                }
            } 
            if(GameType == 3){
                if(Input.GetKeyDown(KeyCode.Space)){
                    if(!ChessAI.gameOver){
                        currentBoard.MakeMove(chessAI.Negamax(chessAI.Depth, currentBoard));
                        UpdateCurrentBoard(false);
                        moveCount ++;
                    }
                }
            }

            if(Input.GetKeyDown(KeyCode.P)){
                Functions.PrintBoard(currentBoard);
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow)){
                currentBoard.UnmakeMove();
                UpdateCurrentBoard(false);
                moveCount--;
            }

            if(Input.GetKeyDown(KeyCode.E)){
                //StartCoroutine(ShowBoards(search.boards));
                print("Evaluation: " + evaluation.GetEvaluation(currentBoard, currentBoard.moveArchive.Peek()));
            }

            if(Input.GetKeyDown(KeyCode.Z)){
                print(Functions.HashPosition(currentBoard));
            }

            if(Input.GetKeyDown(KeyCode.T)){
                Test();
            }

            if(Input.GetKeyDown(KeyCode.R)){
                print(currentBoard.moveArchive.Peek().IsReversible);
            }

            if(Input.GetKeyDown(KeyCode.G)){
                GetFen(currentBoard);
            }
            if(Input.GetKeyDown(KeyCode.M)){
                List<Move> moves = moveGen.GenerateMoves(currentBoard,true);
                foreach(Move move in moves){
                    print(move.MoveName);
                }
            }
            if(Input.GetKeyDown(KeyCode.Q)){
                print("SEE: " + Evaluation.GetSEE(currentBoard,currentBoard.moveArchive.Peek()));
                evaluation.qIterations = 0; 
                (string, int) tup = evaluation.qSearch(currentBoard, minValue, maxValue, evaluation.maxQuieseDepth);
                print("Quiesence: " + (tup.Item2-Evaluation.GetMaterial(currentBoard)) + " || iterations: "+ evaluation.qIterations + " || variation: " + tup.Item1);
            }

            if(Input.GetKeyDown(KeyCode.D)){
                print("is reversible: " + currentBoard.moveArchive.Peek().IsReversible);
                print(Evaluation.CheckRepetition2(currentBoard));
            }

            if(Input.GetKeyDown(KeyCode.N)){
                if(moveGen.TerminalNode(currentBoard)){
                    print("True");
                }
                else{
                    print("False");
                }
            }
        }
        IEnumerator ShowBoards(List<Board> boards){
            foreach(Board board in boards){
                fenScript.LoadPosition(board);
                yield return new WaitForSeconds(wait);
            }
            fenScript.LoadPosition(currentBoard);
        }
        public void GetFen(Board board){
            Dictionary<int,char> dick = new Dictionary<int,char>()
            {
                [0] = '0',
                [1] = 'P', [2] = 'N', [3] = 'B', [4] = 'R', [5] = 'Q', [6] = 'K',
                [7] = 'p', [8] = 'n', [9] = 'b', [10] = 'r', [11] = 'q', [12] = 'k',
            };
            string s = "";
            int[] arr = new int[64];
            for(int i = 0; i< 12; i++){
                ulong p = board.pieces[i];
                while(p != 0){
                    int x = Functions.pop_lsb(ref p);
                    arr[x] = i + 1;
                }
            }
            int c = 0;
            for(int y = 7; y >= 0; y--){
                for(int x = 0; x < 8; x++){
                    if(char.IsLetter(dick[arr[x+y*8]])){
                        if(c != 0){
                            s+= c;
                            c= 0;
                        }
                        s+=dick[arr[x+y*8]];
                    }
                    else{
                        c++;
                    }
                    if(x==7){
                        if(c!=0){
                            s += c;
                            c = 0;
                        }
                        if(y != 0){
                            s += "/";
                        }
                    }
                }
            }

            s += board.whiteTurn ? " w" : " b";
            s+= " - ";
            print("FEN for this position is: " + s);
        }
        public void UpdateCurrentBoard(bool start){
            if(start){
                fenScript.LoadPosition();
            }
            else{
                fenScript.LoadPosition(currentBoard);
            }
            possibleMoves = moveGen.GenerateMoves(currentBoard, false);
            if(possibleMoves.Count == 0){
                ChessAI.gameOver = true;
                if(moveGen.inCheck){
                    if(currentBoard.whiteTurn){
                        print("Black wins");
                    }
                    else{
                        print("White wins");

                    }
                }
                else{
                    print("Draw by stalemate");
                }
            }
            if(Evaluation.CheckRepetition(currentBoard)){
                ChessAI.gameOver = true;
                print("Draw by repetition");
            }
            
            // foreach(ulong[] ulongs in currentBoard.piecesArchive){
            //     print("white: " + (ulongs[0] | ulongs[1] | ulongs[2] | ulongs[3] | ulongs[4] | ulongs[5]) + " , black: " + (ulongs[6] | ulongs[7] | ulongs[8] | ulongs[9] | ulongs[10] | ulongs[11]));
            // }
        }
        public int Perft (Board b, int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            List<Move> moves = moveGen.GenerateMoves(currentBoard, false);
            int numPositions = 0;

            foreach (Move move in moves)
            {
                b.MakeMove(move);
                numPositions += Perft(b, depth - 1);
                b.UnmakeMove();
            }

            return numPositions;
        }
        void Test(){
            int totalTime = 0;
            int count = 0;
            int reps = 1;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < reps; i++)
            { 
                count += Perft(currentBoard, 5);
            }

            watch.Stop();
            totalTime = (int) watch.ElapsedMilliseconds;

            Debug.Log("Average Elapsed Time: " + (totalTime / reps).ToString() + "ms || Moves per second: " + (count * 1000f / totalTime).ToString());
        }
    }
}