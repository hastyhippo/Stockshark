namespace Chess
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public struct Move
	{
		public struct FlagType
		{
			public const int None = 0;
			public const int EnPassant = 1;
			public const int Castling = 2;
			public const int PromoteToKnight = 3;
			public const int PromoteToBishop = 4;
			public const int PromoteToRook = 5;
			public const int PromoteToQueen = 6;
			public const int DoublePush = 7;
		
		}
		public uint moveValue; 
		//  value for  flag
		public const uint startSquareMask = 	0b00000000000000000000000000111111;
		public const uint targetSquareMask =	0b00000000000000000000111111000000;
		public const uint typeMask = 			0b00000000000000001111000000000000;
		public const uint flagMask = 			0b00000000000011110000000000000000;
		public const uint captureMask = 		0b00000000000100000000000000000000;
		public const uint captureTypeMask = 	0b00000000111000000000000000000000;
		public const uint emptyMoveMask =	    0b10000000000000000000000000000000;
		public const uint tempEvalMask =	    0b11111111000000000000000000000000;
		public Move(uint moveValue)
		{
			this.moveValue = moveValue;
		}
		public Move(bool empty){
			moveValue = emptyMoveMask;
		}
		public Move(int startSquare, int targetSquare)
		{
			moveValue = (uint)(startSquare | targetSquare << 6);
		}
		public Move(int startSquare, int targetSquare, int pieceType)
		{
			moveValue = (uint)(startSquare | targetSquare << 6 | pieceType << 12);
		}
		public Move(int startSquare, int targetSquare, int pieceType, int flag)
		{
			moveValue = (uint)(startSquare | targetSquare << 6 | pieceType << 12 | flag << 16);
		}

		public Move(int startSquare, int targetSquare, int pieceType, int flag, int capturedType)
		{
			moveValue = (uint)(startSquare | targetSquare << 6 | pieceType << 12 | flag << 16 | 1 << 20 | capturedType << 21);
		}
		public int SetEval{
			set{
				moveValue |= (uint)(value << 24);
			}
		}
		public bool isEmpty{
			get{
				return (moveValue & emptyMoveMask) != 0;
			}
		}
		public int Eval{
			get{
				return (int)((moveValue & tempEvalMask) >> 24);
			}
		}
		public int StartSquare
		{
			get
			{
				return (int)(moveValue & startSquareMask);
			}
		}

		public bool Capture{
			get{
				return ((moveValue & captureMask) != 0);
			}
		}
		public int CapturedType{
			get{
				return(int)((moveValue & captureTypeMask) >> 21);
			}
		}
		public int TargetSquare
		{
			get
			{
				return (int)(moveValue & targetSquareMask) >> 6;
			}
		}

		public int Type
        {
            get
            {
				return (int)(moveValue & typeMask) >> 12;
            }
        }

		public int Flag
		{
			get
			{
				return (int)(moveValue & flagMask) >> 16;
			}
		}

		public bool IsReversible
		{
			get
			{
				return (Flag != 2 && Type % 6 != 0 && !Capture);
			}
		}
		public static bool SameMove(Move a, Move b)
		{
			return a.moveValue == b.moveValue;
		}

		public string MoveValue{
			get{
				return  " start square: " + StartSquare + " target square: " + TargetSquare;
			}
		}

		public string MoveName{
			get{
				string s = "";
				Dictionary<int, string> pieceNames = new Dictionary<int, string>(){
					[0] = "",
					[1] = "N",
					[2] = "B",
					[3] = "R",
					[4] = "Q",
					[5] = "K",
					[6] = "",
					[7] = "N",
					[8] = "B",
					[9] = "R",
					[10] = "Q",
					[11] = "K"
				};
				s+= Capture? pieceNames[Type] + "x" + Precalculated.squareNames[TargetSquare] : 
				pieceNames[Type] + Precalculated.squareNames[TargetSquare];
				return s;
			}
		}
		public uint Value
		{
			get
			{
				return moveValue;
			}
		}
	}
}