# Stockshark Chess Engine
ChessAI created in C# Unity
First started in 2021, on break since 2022. 
Can serach to a depth of 6 an average, with a branching factor of roughly 19 in the midgame

Features
- Minimax (Negamax variant)
- Alpha-Beta Pruning
- Move Ordering
- Iterative Deepening
- Piece Square Tables
- Zobrist Hashing
- Bitboard Manipulations
- Evaluation heuristics (Pawn structure, possible moves, castling rights etc)

Move Generation
- From the opening position, capable of searching 16 million moves/second using efficient bitboard manipulations and tricks

To add:
- Transposition Tables
- Opening book
- Null move/ Killer move heuristic
- Improved evaluation heuristics (king safety, etc)
