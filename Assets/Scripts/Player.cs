namespace Chess{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        public int depth; 
        public int SIZE;
        public bool displayEvaluation;
        public Vector2 tileOffset;
        public GameObject greenTile;
        public GameObject redTile;
        ulong possibleSquares = 0uL;
        bool hoveringPiece = false;
        int hoveredType = 0;
        int selectedSquare;
        Dictionary<int, uint> squareValue = new Dictionary<int, uint>();
        public FEN fenscript;
        public ChessAI chessAI;
        public MoveGenerator moveGen;
        public GameManager gameManager;
        public DebugSettings debugSetings;
        public Move lastMove;
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Mouse0)){
                TakeInput();
            }
        }
        private void DisplayTiles(GameObject Tile, ulong squares){
            foreach(int i in Functions.BitPositions(squares)){
                Instantiate(Tile, new Vector2((i%8)*2, (i/8)*2+1), Quaternion.identity);
            }
        }
        private void DisplayTiles(GameObject Tile, int square){
            Instantiate(Tile, new Vector2((square%8)*2, (square/8)*2+1), Quaternion.identity);
        }
        private void Reset(){
            hoveringPiece = false;
            possibleSquares = 0uL;
            hoveredType = 0;
            selectedSquare = 0;
            ClearTiles();
            squareValue.Clear();
        }
        public static void ClearTiles(){
            var GO = GameObject.FindGameObjectsWithTag("Tile");
            for (int i = 0; i < GO.Length; i++)
            {
                Destroy(GO[i]);
            }
        }
        public void TakeInput()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.x = worldPos.x/2 + 0.5f;
            worldPos.y /= 2;
            
            Board board = GameManager.currentBoard;

            int clickedSquare = Mathf.CeilToInt(worldPos.x) - 1 + 8* (Mathf.CeilToInt(worldPos.y) - 1);

            ulong friendlyPieces = board.whiteTurn? board.pieces[0] | board.pieces[1] | board.pieces[2] | board.pieces[3] | board.pieces[4] | board.pieces[5] : 
            board.pieces[6] | board.pieces[7] | board.pieces[8] | board.pieces[9] | board.pieces[10] | board.pieces[11];
    
            if(worldPos.x >= 0 && worldPos.x <= 8 && worldPos.y >= 0 && worldPos.y <= 8){
                if(hoveringPiece == false){
                    if((1uL << clickedSquare & friendlyPieces) == 0){
                        Reset();
                        return;
                    }

                    hoveringPiece = true;
                    selectedSquare = clickedSquare;

                    hoveredType = Functions.TypeAtSquare(GameManager.currentBoard, clickedSquare);
                    foreach(Move move in GameManager.possibleMoves){
                        if(move.StartSquare == clickedSquare){
                            possibleSquares |= 1uL << move.TargetSquare;
                            if(move.Flag != 3 && move.Flag != 4 && move.Flag != 5){
                                squareValue.Add(move.TargetSquare, move.moveValue);
                            }
                        }
                    }
                    //ClearTiles();
                    DisplayTiles(greenTile, clickedSquare);
                    DisplayTiles(redTile, possibleSquares);
                }
                else{
                    if(((1uL << clickedSquare) & friendlyPieces) != 0){
                        hoveringPiece = true;
                        selectedSquare = clickedSquare;

                        hoveredType = Functions.TypeAtSquare(GameManager.currentBoard, clickedSquare);

                        possibleSquares = 0uL;

                        squareValue.Clear();

                        foreach(Move move in GameManager.possibleMoves){
                            if(move.StartSquare == clickedSquare){
                                possibleSquares |= 1uL << move.TargetSquare;
                                if(move.Flag != 3 && move.Flag != 4 && move.Flag != 5){
                                    squareValue.Add(move.TargetSquare, move.moveValue);
                                }
                            }
                        }
                        ClearTiles();
                        DisplayTiles(greenTile, clickedSquare);
                        DisplayTiles(redTile, possibleSquares);
                        return;
                    }
                    if(((1uL << clickedSquare) & possibleSquares) == 0){
                        Reset();
                        return;
                    }
                    GameManager.moveCount ++ ;

                    GameManager.currentBoard.MakeMove(new Move(squareValue[clickedSquare]));
                    lastMove = new Move(squareValue[clickedSquare]);

                    gameManager.UpdateCurrentBoard(false);

                    if(displayEvaluation){
                        chessAI.Negamax(depth, GameManager.currentBoard);
                    }

                    if(GameManager.possibleMoves.Count == 0){
                        if(moveGen.inCheck){
                            if(GameManager.currentBoard.whiteTurn){
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
                    Reset();                
                }
            }
        }
        bool check_win(int[][] map, bool game_over){
        for(int x = 0; x< SIZE; x++){
            for(int y = 0; y < SIZE; y++){
                if(map[x][y] != 0){
                    return false;
                }
            }
        }
        return true;
    }
    }


}