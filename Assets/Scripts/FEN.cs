namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FEN : MonoBehaviour
    {
        public string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq";
        public string Fen;

        public bool startingPosition;
        public MoveGenerator moveGEN;
        public GameManager gameManager;

        public GameObject[] boardPieces = new GameObject[12];

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                LoadPosition();
            }
        }

        public void LoadPosition()
        {
            Board newPosition = startingPosition? ConvertFEN(startingFEN) : ConvertFEN(Fen);
            GameManager.currentBoard = newPosition;
            BuildPoistion(newPosition);
        }
        public void LoadPosition(Board newPosition)
        {
            BuildPoistion(newPosition);
        }

        

        public static Board ConvertFEN(string FEN)
        {
            Board newState = new Board();
            newState.gameState = 0;
            Dictionary<char,int> pieceSymbols = new Dictionary<char, int>()
            {
                ['p'] = 0,
                ['n'] = 1,
                ['b'] = 2,
                ['r'] = 3,
                ['q'] = 4,
                ['k'] = 5
            };
            string FENCode = FEN.Split()[0];
            
            int file = 0, rank = 7;
            foreach (char character in FENCode)
            {
                if (character == '/')
                {
                    rank--;
                    file = 0;
                }
                else
                {
                    if (char.IsDigit(character))
                    {
                        file += (int)char.GetNumericValue(character);
                    }
                    else
                    {
                        int pieceType = char.IsUpper(character) ? pieceSymbols[char.ToLower(character)] : pieceSymbols[char.ToLower(character)] + 6;
                        newState.pieces[pieceType] |= 1uL << rank * 8 + file;
                        file++;
                    }
                }
            }

            string colour = FEN.Split()[1];

            if(colour == "w" || colour == "W")
            {
                newState.whiteTurn = true;
            }
            else if (colour == "b" || colour == "B")
            {
                newState.whiteTurn = false;
            }
            else
            {
                print("Invalid colour in FEN");
            }

            string CastlingRights = FEN.Split()[2];
            if(CastlingRights.Contains("K")){
                newState.gameState |= 0b0000000000000001;
            }
            if(CastlingRights.Contains("Q")){
                newState.gameState |= 0b0000000000000010;
            }
            if(CastlingRights.Contains("k")){
                newState.gameState |= 0b0000000000000100;
            }
            if(CastlingRights.Contains("q")){
                newState.gameState |= 0b0000000000001000;
            }
            
            newState.ZobristArchive.Push(Functions.HashPosition(newState));
            return newState;

        }

        public void BuildPoistion(Board boardState)
        {
            GameObject[] allBoardPieces = GameObject.FindGameObjectsWithTag("Piece");
            foreach (GameObject obj in allBoardPieces)
            {
                Destroy(obj);
            }
            for (int i = 0; i < 12; i++)
            {
                foreach (int pos in Functions.BitPositions(boardState.pieces[i]))
                {
                    Instantiate(boardPieces[i], new Vector2(pos % 8 * 2, pos / 8 * 2 + 1), Quaternion.identity);
                }
            }

        }
    }

}