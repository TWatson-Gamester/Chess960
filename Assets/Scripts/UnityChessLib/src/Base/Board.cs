using System;
using System.Collections.Generic;

namespace UnityChess {
	/// <summary>An 8x8 matrix representation of a chessboard.</summary>
	public class Board {
		private readonly Piece[,] boardMatrix;
		private readonly Dictionary<Side, Square?> currentKingSquareBySide = new Dictionary<Side, Square?> {
			[Side.White] = null,
			[Side.Black] = null
		};

		public Piece this[Square position] {
			get {
				if (position.IsValid()) return boardMatrix[position.File - 1, position.Rank - 1];
				throw new ArgumentOutOfRangeException($"Position was out of range: {position}");
			}

			set {
				if (position.IsValid()) boardMatrix[position.File - 1, position.Rank - 1] = value;
				else throw new ArgumentOutOfRangeException($"Position was out of range: {position}");
			}
		}

		public Piece this[int file, int rank] {
			get => this[new Square(file, rank)];
			set => this[new Square(file, rank)] = value;
		}

		/// <summary>Creates a Board given the passed square-piece pairs.</summary>
		public Board(params (Square, Piece)[] squarePiecePairs) {
			boardMatrix = new Piece[8, 8];

			foreach ((Square position, Piece piece) in squarePiecePairs) {
				this[position] = piece;
			}
		}

		/// <summary>Creates a deep copy of the passed Board.</summary>
		public Board(Board board) {
			// TODO optimize this method
			// Creates deep copy (makes copy of each piece and deep copy of their respective ValidMoves lists) of board (list of BasePiece's)
			// this may be a memory hog since each Board has a list of Piece's, and each piece has a list of Movement's
			// avg number turns/Board's per game should be around ~80. usual max number of pieces per board is 32
			boardMatrix = new Piece[8, 8];
			for (int file = 1; file <= 8; file++) {
				for (int rank = 1; rank <= 8; rank++) {
					Piece pieceToCopy = board[file, rank];
					if (pieceToCopy == null) { continue; }

					this[file, rank] = pieceToCopy.DeepCopy();
				}
			}
		}

		public void ClearBoard() {
			for (int file = 1; file <= 8; file++) {
				for (int rank = 1; rank <= 8; rank++) {
					this[file, rank] = null;
				}
			}

			currentKingSquareBySide[Side.White] = null;
			currentKingSquareBySide[Side.Black] = null;
		}

		public static string TestRandomPlacement()
		{
			//Place the king at a random position from b1 - g1
			int kingLocation = new Random().Next(2, 8);

			//Places the rooks at a random position on both sides of the King
			int leftRookLocation = new Random().Next(1, kingLocation);
			int rightRookLocation = new Random().Next(kingLocation + 1, 9);

			//Places the bishops at random spots but making sure that they are only on white & black places
			bool canContinue = false;
			int bishop1Random = new Random().Next(1, 5);
			int bishop2Random = new Random().Next(1, 5);
			int whiteBishopLocation = 0;
			int blackBishopLocation = 0;
			while (!canContinue)
			{
				switch (bishop1Random)
				{
					case 1:
						whiteBishopLocation = 2;
						break;
					case 2:
						whiteBishopLocation = 4;
						break;
					case 3:
						whiteBishopLocation = 6;
						break;
					case 4:
						whiteBishopLocation = 8;
						break;
				}
				switch (bishop1Random)
				{
					case 1:
						blackBishopLocation = 1;
						break;
					case 2:
						blackBishopLocation = 3;
						break;
					case 3:
						blackBishopLocation = 5;
						break;
					case 4:
						blackBishopLocation = 7;
						break;
				}

				if (whiteBishopLocation != kingLocation && whiteBishopLocation != leftRookLocation && whiteBishopLocation != rightRookLocation) canContinue = true;
				else
				{
					bishop1Random = new Random().Next(1, 4);
					canContinue = false;
				}

				if (blackBishopLocation != kingLocation && blackBishopLocation != leftRookLocation && blackBishopLocation != rightRookLocation) canContinue = true;
				else
				{
					bishop2Random = new Random().Next(1, 4);
					canContinue = false;
				}
			}

			//Places Queen on a random space
			canContinue = false;
			int queenLocation = 0;
			while (!canContinue)
			{
				queenLocation = new Random().Next(1, 9);
				if (queenLocation != kingLocation && queenLocation != leftRookLocation && queenLocation != rightRookLocation && queenLocation != whiteBishopLocation && queenLocation != blackBishopLocation) canContinue = true;
			}
			canContinue = false;

			//Places the Knights on the last 2 remaining spaces
			int knight1Location = 0;
			while (!canContinue)
			{
				knight1Location = new Random().Next(1, 9);
				if (knight1Location != kingLocation && knight1Location != leftRookLocation && knight1Location != rightRookLocation && knight1Location != whiteBishopLocation && knight1Location != blackBishopLocation && knight1Location != queenLocation) canContinue = true;
			}
			canContinue = false;
			int knight2Location = 0;
			while (!canContinue)
			{
				knight2Location = new Random().Next(1, 9);
				if (knight2Location != kingLocation && knight2Location != leftRookLocation && knight2Location != rightRookLocation && knight2Location != whiteBishopLocation && knight2Location != blackBishopLocation && knight2Location != queenLocation && knight2Location != knight1Location) canContinue = true;
			}

			char[] stringBuilder = new char[8];
			/**
			 * King == 1
			 * Queen == 2
			 * Bishop == 3
			 * Knight == 4
			 * Rook == 5
			 */
			stringBuilder[kingLocation - 1] = '1';
			stringBuilder[queenLocation - 1] = '2';
			stringBuilder[whiteBishopLocation - 1] = '3';
			stringBuilder[blackBishopLocation - 1] = '3';
			stringBuilder[knight1Location - 1] = '4';
			stringBuilder[knight2Location - 1] = '4';
			stringBuilder[leftRookLocation - 1] = '5';
			stringBuilder[rightRookLocation - 1] = '5';

			string returnString = "";
			foreach (char c in stringBuilder) returnString += c;
			return returnString;
        }

		public static (Square, Piece)[] RandomStart()
        {
			//Place the king at a random position from b1 - g1
			int kingLocation = new Random().Next(2, 8);

			//Places the rooks at a random position on both sides of the King
			int leftRookLocation = new Random().Next(1, kingLocation);
			int rightRookLocation = new Random().Next(kingLocation + 1, 9);

			//Places the bishops at random spots but making sure that they are only on white & black places
			bool canContinue = false;
			int bishop1Random = new Random().Next(1, 5);
			int bishop2Random = new Random().Next(1, 5);
			int whiteBishopLocation = 0;
			int blackBishopLocation = 0;
			while (!canContinue)
            {
				switch (bishop1Random)
				{
					case 1:
						whiteBishopLocation = 2;
						break;
					case 2:
						whiteBishopLocation = 4;
						break;
					case 3:
						whiteBishopLocation = 6;
						break;
					case 4:
						whiteBishopLocation = 8;
						break;
				}
				switch (bishop1Random)
				{
					case 1:
						blackBishopLocation = 1;
						break;
					case 2:
						blackBishopLocation = 3;
						break;
					case 3:
						blackBishopLocation = 5;
						break;
					case 4:
						blackBishopLocation = 7;
						break;
				}

				if (whiteBishopLocation != kingLocation && whiteBishopLocation != leftRookLocation && whiteBishopLocation != rightRookLocation) canContinue = true;
				else
				{
					bishop1Random = new Random().Next(1, 4);
					canContinue = false;
				}

				if (blackBishopLocation != kingLocation && blackBishopLocation != leftRookLocation && blackBishopLocation != rightRookLocation) canContinue = true;
				else
				{
					bishop2Random = new Random().Next(1, 4);
					canContinue = false;
				}
			}

			//Places Queen on a random space
			canContinue = false;
			int queenLocation = 0;
			while (!canContinue)
            {
				queenLocation = new Random().Next(1, 9);
				if (queenLocation != kingLocation && queenLocation != leftRookLocation && queenLocation != rightRookLocation && queenLocation != whiteBishopLocation && queenLocation != blackBishopLocation) canContinue = true;
            }
			canContinue = false;

			//Places the Knights on the last 2 remaining spaces
			int knight1Location = 0;
			while (!canContinue)
			{
				knight1Location = new Random().Next(1, 9);
				if (knight1Location != kingLocation && knight1Location != leftRookLocation && knight1Location != rightRookLocation && knight1Location != whiteBishopLocation && knight1Location != blackBishopLocation && knight1Location != queenLocation) canContinue = true;
			}
			canContinue = false;
			int knight2Location = 0;
			while (!canContinue)
			{
				knight2Location = new Random().Next(1, 9);
				if (knight2Location != kingLocation && knight2Location != leftRookLocation && knight2Location != rightRookLocation && knight2Location != whiteBishopLocation && knight2Location != blackBishopLocation && knight2Location != queenLocation && knight2Location != knight1Location) canContinue = true;
			}

			List<(Square, Piece)> randomStartingPositionPieces = new List<(Square, Piece)>();
			randomStartingPositionPieces.Add((new Square("a2"), new Pawn(Side.White)));
			randomStartingPositionPieces.Add((new Square("b2"), new Pawn(Side.White)));
			randomStartingPositionPieces.Add((new Square("c2"), new Pawn(Side.White)));
			randomStartingPositionPieces.Add((new Square("d2"), new Pawn(Side.White)));
			randomStartingPositionPieces.Add((new Square("e2"), new Pawn(Side.White)));
			randomStartingPositionPieces.Add((new Square("f2"), new Pawn(Side.White)));
			randomStartingPositionPieces.Add((new Square("g2"), new Pawn(Side.White)));
			randomStartingPositionPieces.Add((new Square("h2"), new Pawn(Side.White)));

			randomStartingPositionPieces.Add((new Square("a7"), new Pawn(Side.Black)));
			randomStartingPositionPieces.Add((new Square("b7"), new Pawn(Side.Black)));
			randomStartingPositionPieces.Add((new Square("c7"), new Pawn(Side.Black)));
			randomStartingPositionPieces.Add((new Square("d7"), new Pawn(Side.Black)));
			randomStartingPositionPieces.Add((new Square("e7"), new Pawn(Side.Black)));
			randomStartingPositionPieces.Add((new Square("f7"), new Pawn(Side.Black)));
			randomStartingPositionPieces.Add((new Square("g7"), new Pawn(Side.Black)));
			randomStartingPositionPieces.Add((new Square("h7"), new Pawn(Side.Black)));

            switch (kingLocation)
            {
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new King(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new King(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new King(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new King(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new King(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new King(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new King(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new King(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new King(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new King(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new King(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new King(Side.Black)));
					break;
            }

			switch (leftRookLocation)
			{
				case 1:
					randomStartingPositionPieces.Add((new Square("a1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("a8"), new Rook(Side.Black)));
					break;
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new Rook(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new Rook(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new Rook(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new Rook(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new Rook(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new Rook(Side.Black)));
					break;
				case 8:
					randomStartingPositionPieces.Add((new Square("h1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("h8"), new Rook(Side.Black)));
					break;
			}

			switch (rightRookLocation)
			{
				case 1:
					randomStartingPositionPieces.Add((new Square("a1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("a8"), new Rook(Side.Black)));
					break;
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new Rook(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new Rook(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new Rook(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new Rook(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new Rook(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new Rook(Side.Black)));
					break;
				case 8:
					randomStartingPositionPieces.Add((new Square("h1"), new Rook(Side.White)));
					randomStartingPositionPieces.Add((new Square("h8"), new Rook(Side.Black)));
					break;
			}

			switch (whiteBishopLocation)
			{
				case 1:
					randomStartingPositionPieces.Add((new Square("a1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("a8"), new Bishop(Side.Black)));
					break;
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new Bishop(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new Bishop(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new Bishop(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new Bishop(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new Bishop(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new Bishop(Side.Black)));
					break;
				case 8:
					randomStartingPositionPieces.Add((new Square("h1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("h8"), new Bishop(Side.Black)));
					break;
			}

			switch (blackBishopLocation)
			{
				case 1:
					randomStartingPositionPieces.Add((new Square("a1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("a8"), new Bishop(Side.Black)));
					break;
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new Bishop(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new Bishop(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new Bishop(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new Bishop(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new Bishop(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new Bishop(Side.Black)));
					break;
				case 8:
					randomStartingPositionPieces.Add((new Square("h1"), new Bishop(Side.White)));
					randomStartingPositionPieces.Add((new Square("h8"), new Bishop(Side.Black)));
					break;
			}

			switch (queenLocation)
			{
				case 1:
					randomStartingPositionPieces.Add((new Square("a1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("a8"), new Queen(Side.Black)));
					break;
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new Queen(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new Queen(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new Queen(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new Queen(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new Queen(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new Queen(Side.Black)));
					break;
				case 8:
					randomStartingPositionPieces.Add((new Square("h1"), new Queen(Side.White)));
					randomStartingPositionPieces.Add((new Square("h8"), new Queen(Side.Black)));
					break;
			}

			switch (knight1Location)
			{
				case 1:
					randomStartingPositionPieces.Add((new Square("a1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("a8"), new Knight(Side.Black)));
					break;
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new Knight(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new Knight(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new Knight(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new Knight(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new Knight(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new Knight(Side.Black)));
					break;
				case 8:
					randomStartingPositionPieces.Add((new Square("h1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("h8"), new Knight(Side.Black)));
					break;
			}

			switch (knight2Location)
			{
				case 1:
					randomStartingPositionPieces.Add((new Square("a1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("a8"), new Knight(Side.Black)));
					break;
				case 2:
					randomStartingPositionPieces.Add((new Square("b1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("b8"), new Knight(Side.Black)));
					break;
				case 3:
					randomStartingPositionPieces.Add((new Square("c1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("c8"), new Knight(Side.Black)));
					break;
				case 4:
					randomStartingPositionPieces.Add((new Square("d1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("d8"), new Knight(Side.Black)));
					break;
				case 5:
					randomStartingPositionPieces.Add((new Square("e1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("e8"), new Knight(Side.Black)));
					break;
				case 6:
					randomStartingPositionPieces.Add((new Square("f1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("f8"), new Knight(Side.Black)));
					break;
				case 7:
					randomStartingPositionPieces.Add((new Square("g1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("g8"), new Knight(Side.Black)));
					break;
				case 8:
					randomStartingPositionPieces.Add((new Square("h1"), new Knight(Side.White)));
					randomStartingPositionPieces.Add((new Square("h8"), new Knight(Side.Black)));
					break;
			}

			return randomStartingPositionPieces.ToArray();
		}

		public static readonly (Square, Piece)[] StartingPositionPieces = {
			(new Square("a1"), new Rook(Side.White)),
			(new Square("b1"), new Knight(Side.White)),
			(new Square("c1"), new Bishop(Side.White)),
			(new Square("d1"), new Queen(Side.White)),
			(new Square("e1"), new King(Side.White)),
			(new Square("f1"), new Bishop(Side.White)),
			(new Square("g1"), new Knight(Side.White)),
			(new Square("h1"), new Rook(Side.White)),
			
			(new Square("a2"), new Pawn(Side.White)),
			(new Square("b2"), new Pawn(Side.White)),
			(new Square("c2"), new Pawn(Side.White)),
			(new Square("d2"), new Pawn(Side.White)),
			(new Square("e2"), new Pawn(Side.White)),
			(new Square("f2"), new Pawn(Side.White)),
			(new Square("g2"), new Pawn(Side.White)),
			(new Square("h2"), new Pawn(Side.White)),
			
			(new Square("a8"), new Rook(Side.Black)),
			(new Square("b8"), new Knight(Side.Black)),
			(new Square("c8"), new Bishop(Side.Black)),
			(new Square("d8"), new Queen(Side.Black)),
			(new Square("e8"), new King(Side.Black)),
			(new Square("f8"), new Bishop(Side.Black)),
			(new Square("g8"), new Knight(Side.Black)),
			(new Square("h8"), new Rook(Side.Black)),
			
			(new Square("a7"), new Pawn(Side.Black)),
			(new Square("b7"), new Pawn(Side.Black)),
			(new Square("c7"), new Pawn(Side.Black)),
			(new Square("d7"), new Pawn(Side.Black)),
			(new Square("e7"), new Pawn(Side.Black)),
			(new Square("f7"), new Pawn(Side.Black)),
			(new Square("g7"), new Pawn(Side.Black)),
			(new Square("h7"), new Pawn(Side.Black)),
		};

		public void MovePiece(Movement move) {
			if (this[move.Start] is not { } pieceToMove) {
				throw new ArgumentException($"No piece was found at the given position: {move.Start}");
			}

			this[move.Start] = null;
			this[move.End] = pieceToMove;

			if (pieceToMove is King) {
				currentKingSquareBySide[pieceToMove.Owner] = move.End;
			}

			(move as SpecialMove)?.HandleAssociatedPiece(this);
		}
		
		internal bool IsOccupiedAt(Square position) => this[position] != null;

		internal bool IsOccupiedBySideAt(Square position, Side side) => this[position] is Piece piece && piece.Owner == side;

		public Square GetKingSquare(Side player) {
			if (currentKingSquareBySide[player] == null) {
				for (int file = 1; file <= 8; file++) {
					for (int rank = 1; rank <= 8; rank++) {
						if (this[file, rank] is King king) {
							currentKingSquareBySide[king.Owner] = new Square(file, rank);
						}
					}
				}
			}

			return currentKingSquareBySide[player] ?? Square.Invalid;
		}

		public string ToTextArt() {
			string result = string.Empty;
			
			for (int rank = 8; rank >= 1; --rank) {
				for (int file = 1; file <= 8; ++file) {
					Piece piece = this[file, rank];
					result += piece.ToTextArt();
					result += file != 8
						? "|"
						: $"\t {rank}";
				}

				result += "\n";
			}
			
			result += "a b c d e f g h";

			return result;
		} 
	}
}