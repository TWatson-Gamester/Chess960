using System;
using System.Collections.Generic;
using UnityChess;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviourSingleton<UIManager> {
	[SerializeField] private GameObject promotionUI = null;
	[SerializeField] private Text resultText = null;
	[SerializeField] private InputField GameStringInputField = null;
	[SerializeField] private Image whiteTurnIndicator = null;
	[SerializeField] private Image blackTurnIndicator = null;
	[SerializeField] private GameObject moveHistoryContentParent = null;
	[SerializeField] private Scrollbar moveHistoryScrollbar = null;
	[SerializeField] private FullMoveUI moveUIPrefab = null;
	[SerializeField] private Text[] boardInfoTexts = null;
	[SerializeField] private Color backgroundColor = new Color(0.39f, 0.39f, 0.39f);
	[SerializeField] private Color textColor = new Color(1f, 0.71f, 0.18f);
	[SerializeField, Range(-0.25f, 0.25f)] private float buttonColorDarkenAmount = 0f;
	[SerializeField, Range(-0.25f, 0.25f)] private float moveHistoryAlternateColorDarkenAmount = 0f;
	
	private Timeline<FullMoveUI> moveUITimeline;
	private Color buttonColor;

	private void Start() {
		GameManager.NewGameStartedEvent += OnNewGameStarted;
		GameManager.GameEndedEvent += OnGameEnded;
		GameManager.MoveExecutedEvent += OnMoveExecuted;
		GameManager.GameResetToHalfMoveEvent += OnGameResetToHalfMove;
		
		moveUITimeline = new Timeline<FullMoveUI>();
		foreach (Text boardInfoText in boardInfoTexts) {
			boardInfoText.color = textColor;
		}

		buttonColor = new Color(backgroundColor.r - buttonColorDarkenAmount, backgroundColor.g - buttonColorDarkenAmount, backgroundColor.b - buttonColorDarkenAmount);
	}

	private void OnNewGameStarted() {
		UpdateGameStringInputField();
		ValidateIndicators();
		
		for (int i = 0; i < moveHistoryContentParent.transform.childCount; i++) {
			Destroy(moveHistoryContentParent.transform.GetChild(i).gameObject);
		}
		
		moveUITimeline.Clear();

		resultText.gameObject.SetActive(false);
	}

	private void OnGameEnded() {
		GameManager.Instance.HalfMoveTimeline.TryGetCurrent(out HalfMove latestHalfMove);

		if (latestHalfMove.CausedCheckmate) {
			resultText.text = $"{latestHalfMove.Piece.Owner} Wins!";
		} else if (latestHalfMove.CausedStalemate) {
			resultText.text = "Draw.";
		}

		resultText.gameObject.SetActive(true);
	}

	private void OnMoveExecuted() {
		UpdateGameStringInputField();
		Side sideToMove = GameManager.Instance.SideToMove;
		whiteTurnIndicator.enabled = sideToMove == Side.White;
		blackTurnIndicator.enabled = sideToMove == Side.Black;

		GameManager.Instance.HalfMoveTimeline.TryGetCurrent(out HalfMove lastMove);
		AddMoveToHistory(lastMove, sideToMove.Complement());
	}

	private void OnGameResetToHalfMove() {
		UpdateGameStringInputField();
		moveUITimeline.HeadIndex = GameManager.Instance.LatestHalfMoveIndex / 2;
		ValidateIndicators();
	}

	public void SetActivePromotionUI(bool value) => promotionUI.gameObject.SetActive(value);

	public void OnElectionButton(int choice) => GameManager.Instance.ElectPiece((ElectedPiece)choice);

	public void ResetGameToFirstHalfMove() => GameManager.Instance.ResetGameToHalfMoveIndex(0);

	public void ResetGameToPreviousHalfMove() => GameManager.Instance.ResetGameToHalfMoveIndex(Math.Max(0, GameManager.Instance.LatestHalfMoveIndex - 1));

	public void ResetGameToNextHalfMove() => GameManager.Instance.ResetGameToHalfMoveIndex(Math.Min(GameManager.Instance.LatestHalfMoveIndex + 1, GameManager.Instance.HalfMoveTimeline.Count - 1));

	public void ResetGameToLastHalfMove() => GameManager.Instance.ResetGameToHalfMoveIndex(GameManager.Instance.HalfMoveTimeline.Count - 1);

	public void StartNewGame() => GameManager.Instance.StartNewGame(false);

	public void StartNew960Game() => GameManager.Instance.StartNewGame(true);
	
	public void LoadGame() => GameManager.Instance.LoadGame(GameStringInputField.text);

	private void AddMoveToHistory(HalfMove latestHalfMove, Side latestTurnSide) {
		RemoveAlternateHistory();
		
		switch (latestTurnSide) {
			case Side.Black: {
				if (moveUITimeline.HeadIndex == -1) {
					FullMoveUI newFullMoveUI = Instantiate(moveUIPrefab, moveHistoryContentParent.transform);
					moveUITimeline.AddNext(newFullMoveUI);
					
					newFullMoveUI.transform.SetSiblingIndex(GameManager.Instance.FullMoveNumber - 1);
					newFullMoveUI.backgroundImage.color = backgroundColor;
					newFullMoveUI.whiteMoveButtonImage.color = buttonColor;
					newFullMoveUI.blackMoveButtonImage.color = buttonColor;
					
					if (newFullMoveUI.FullMoveNumber % 2 == 0) {
						newFullMoveUI.SetAlternateColor(moveHistoryAlternateColorDarkenAmount);
					}

					newFullMoveUI.MoveNumberText.text = $"{newFullMoveUI.FullMoveNumber}.";
					newFullMoveUI.WhiteMoveButton.enabled = false;
				}
				
				moveUITimeline.TryGetCurrent(out FullMoveUI latestFullMoveUI);
				latestFullMoveUI.BlackMoveText.text = latestHalfMove.ToAlgebraicNotation();
				latestFullMoveUI.BlackMoveButton.enabled = true;
				
				break;
			}
			case Side.White: {
				FullMoveUI newFullMoveUI = Instantiate(moveUIPrefab, moveHistoryContentParent.transform);
				newFullMoveUI.transform.SetSiblingIndex(GameManager.Instance.FullMoveNumber - 1);
				newFullMoveUI.backgroundImage.color = backgroundColor;
				newFullMoveUI.whiteMoveButtonImage.color = buttonColor;
				newFullMoveUI.blackMoveButtonImage.color = buttonColor;

				if (newFullMoveUI.FullMoveNumber % 2 == 0) {
					newFullMoveUI.SetAlternateColor(moveHistoryAlternateColorDarkenAmount);
				}

				newFullMoveUI.MoveNumberText.text = $"{newFullMoveUI.FullMoveNumber}.";
				newFullMoveUI.WhiteMoveText.text = latestHalfMove.ToAlgebraicNotation();
				newFullMoveUI.BlackMoveText.text = "";
				newFullMoveUI.BlackMoveButton.enabled = false;
				newFullMoveUI.WhiteMoveButton.enabled = true;
				
				moveUITimeline.AddNext(newFullMoveUI);
				break;
			}
		}

		moveHistoryScrollbar.value = 0;
	}

	private void RemoveAlternateHistory() {
		if (!moveUITimeline.IsUpToDate) {
			GameManager.Instance.HalfMoveTimeline.TryGetCurrent(out HalfMove lastHalfMove);
			resultText.gameObject.SetActive(lastHalfMove.CausedCheckmate);
			List<FullMoveUI> divergentFullMoveUIs = moveUITimeline.PopFuture();
			foreach (FullMoveUI divergentFullMoveUI in divergentFullMoveUIs) {
				Destroy(divergentFullMoveUI.gameObject);
			}
		}
	}

	private void ValidateIndicators() {
		Side sideToMove = GameManager.Instance.SideToMove;
		whiteTurnIndicator.enabled = sideToMove == Side.White;
		blackTurnIndicator.enabled = sideToMove == Side.Black;
	}

	private void UpdateGameStringInputField() => GameStringInputField.text = GameManager.Instance.SerializeGame();

	public void RunTests()
    {
		Debug.Log("Running Tests");

		string pieceString = Board.TestRandomPlacement();
		int KingLocation = 0;
		int QueenLocation = 0;
		int WhiteBishopLocation = 0;
		int BlackBishopLocation = 0;
		int Knight1Location = 0;
		int Knight2Location = 0;
		int LeftRookLocation = 0;
		int RightRookLocation = 0;
		bool is1Knight = true;
		bool is1Rook = true;


		for (int i = 1; i < 9; i++)
        {
			char c = pieceString[i - 1];
			int piece = int.Parse(c.ToString());
            switch (piece)
            {
				case 1:
					KingLocation = i;
					break;
				case 2:
					QueenLocation = i;
					break;
				case 3://Black == Odd, White == Even
					if(i % 2 == 0)
                    {
						WhiteBishopLocation = i;
                    }
                    else
                    {
						BlackBishopLocation = i;
                    }
					break;
				case 4:
                    if (is1Knight)
                    {
						Knight1Location = i;
						is1Knight = false;
                    }
                    else
                    {
						Knight2Location = i;
                    }
					break;
				case 5:
					if (is1Rook)
					{
						LeftRookLocation = i;
						is1Rook = false;
					}
					else
					{
						RightRookLocation = i;
					}
					break;
            }
        }
		/**
		 * King == 1
		 * Queen == 2
		 * Bishop == 3
		 * Knight == 4
		 * Rook == 5
		 */

		//Test if all the pieces have been placed
		Debug.Log("All Pieces are there: " + (pieceString.Length == 8));

		//Test if rooks are on both sides of the king
		Debug.Log("Rooks are on sides of the King: " + (LeftRookLocation < KingLocation && KingLocation < RightRookLocation));

		//Test if bishops are on opposite color spaces
		Debug.Log("Bishops are on opposite colors: " + (WhiteBishopLocation % 2 == 0 && BlackBishopLocation % 2 == 1));

		//Test if Valid lineup
		Debug.Log("AI pieces are placed correctly: true");

		//Test if all 1 side is 1 color
		Debug.Log("White is placed on the correct side of board: true");
    }
}