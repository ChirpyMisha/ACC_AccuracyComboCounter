#nullable enable
using System;
using System.Collections.Generic;
using ACC.Configuration;
using Zenject;
using static NoteData;

namespace ACC.Core
{
	class AccTracker : IInitializable, IDisposable, ICutScoreBufferDidChangeReceiver, ICutScoreBufferDidFinishReceiver
	{
		private Dictionary<ISaberSwingRatingCounter, NoteCutInfo> swingCounterCutInfo = new Dictionary<ISaberSwingRatingCounter, NoteCutInfo>();

		private readonly ScoreController? scoreController;
		private readonly BeatmapObjectManager? beatmapObjectManager;
		private readonly PlayerHeadAndObstacleInteraction? obstacleInteraction;
		private readonly AccManager accManager;
		private readonly PluginConfig config;

		public AccTracker([InjectOptional] ScoreController scoreController, [InjectOptional] BeatmapObjectManager beatmapObjectManager, [InjectOptional] PlayerHeadAndObstacleInteraction obstacleInteraction, AccManager accManager)
		{
			this.scoreController = scoreController;
			this.beatmapObjectManager = beatmapObjectManager;
			this.obstacleInteraction = obstacleInteraction;
			this.accManager = accManager;
			config = PluginConfig.Instance;
		}

		public void Initialize()
		{
			// Reset the AccThresholdManager's stored values
			accManager.Reset();

			// Assign events
			if (scoreController != null)
			{
				scoreController.scoringForNoteStartedEvent += ScoreController_scoringForNoteStartedEvent;
			}
			if (beatmapObjectManager != null)
			{
				beatmapObjectManager.noteWasCutEvent += BeatmapObjectManager_noteWasCutEvent;
				beatmapObjectManager.noteWasMissedEvent += BeatmapObjectManager_noteWasMissedEvent;
			}
			if (obstacleInteraction != null)
			{
				obstacleInteraction.headDidEnterObstacleEvent += ObstacleInteraction_headDidEnterObstacleEvent;
			}
		}

		public void Dispose()
		{
			// Unassign events
			if (scoreController != null)
			{
				scoreController.scoringForNoteStartedEvent -= ScoreController_scoringForNoteStartedEvent;
			}
			if (beatmapObjectManager != null)
			{
				beatmapObjectManager.noteWasMissedEvent -= BeatmapObjectManager_noteWasMissedEvent;
				beatmapObjectManager.noteWasMissedEvent -= BeatmapObjectManager_noteWasMissedEvent;
			}
			if (obstacleInteraction != null)
			{
				obstacleInteraction.headDidEnterObstacleEvent -= ObstacleInteraction_headDidEnterObstacleEvent;
			}
		}

		private void ObstacleInteraction_headDidEnterObstacleEvent(ObstacleController _) => accManager.BreakCombo(BrokenComboType.HeadIsInObstacle);

		private void BeatmapObjectManager_noteWasMissedEvent(NoteController noteController) => EvaluateMiss(noteController.noteData);
		private void BeatmapObjectManager_noteWasCutEvent(NoteController noteController, in NoteCutInfo noteCutInfo) => EvaluateCut(noteController, noteCutInfo);
		private void ScoreController_scoringForNoteStartedEvent(ScoringElement scoringElement) => EvaluateScoringStart(scoringElement);

		private void EvaluateMiss(NoteData noteData)
		{
			if (IsBomb(noteData))
				return;

			accManager.BreakCombo(BrokenComboType.Miss);
		}

		private void EvaluateCut(NoteController noteController, NoteCutInfo noteCutInfo)
		{
			NoteData noteData = noteController.noteData;

			if (IsHeadInObstacle)
				accManager.BreakCombo(BrokenComboType.HeadIsInObstacle);
			else if (IsBomb(noteData))
				accManager.BreakCombo(BrokenComboType.BombCut);
			else if (IsCutBad(noteCutInfo))
				accManager.BreakCombo(BrokenComboType.BadCut);
		}

		private void EvaluateScoringStart(ScoringElement scoringElement)
		{
			if (scoringElement is GoodCutScoringElement goodCutScoringElement && !IsHeadInObstacle)
			{
				if (IsBurstSliderElement(goodCutScoringElement))
					accManager.IncreaseCombo(IncreaseComboType.OnCut);
				else if (IsCutUnableToExceedThreshold(goodCutScoringElement))
					accManager.BreakCombo(BrokenComboType.BelowThresholdOnCut);
				else if (IsScoringFinished(goodCutScoringElement) && IsCutAboveThreshold(goodCutScoringElement))
					accManager.IncreaseCombo(IncreaseComboType.OnCut);
				else
				{
					accManager.IncreaseCombo(IncreaseComboType.ProvisionalOnCut);
					goodCutScoringElement.cutScoreBuffer.RegisterDidChangeReceiver(this);
					goodCutScoringElement.cutScoreBuffer.RegisterDidFinishReceiver(this);
				}
			}
		}

		public void HandleCutScoreBufferDidChange(CutScoreBuffer cutScoreBuffer)
		{
			if (!IsCutAboveThreshold(cutScoreBuffer))
				return;

			accManager.IncreaseCombo(IncreaseComboType.ProvisionalFinish);
			cutScoreBuffer.UnregisterDidChangeReceiver(this);
			cutScoreBuffer.UnregisterDidFinishReceiver(this);
		}

		public void HandleCutScoreBufferDidFinish(CutScoreBuffer cutScoreBuffer)
		{
			if (!IsCutAboveThreshold(cutScoreBuffer))
				accManager.BreakCombo(BrokenComboType.BelowThresholdOnFinish);
			else
				accManager.IncreaseCombo(IncreaseComboType.ProvisionalFinish);

			cutScoreBuffer.UnregisterDidChangeReceiver(this);
			cutScoreBuffer.UnregisterDidFinishReceiver(this);
		}

		private bool IsScoringFinished(GoodCutScoringElement goodCutScoringElement) => goodCutScoringElement.cutScoreBuffer.isFinished;
		private bool IsHeadInObstacle => obstacleInteraction != null && obstacleInteraction.playerHeadIsInObstacle;
		private bool IsBurstSliderElement(ScoringElement scoringElement) => IsBurstSliderElement(scoringElement.noteData);
		private bool IsBurstSliderElement(NoteData noteData) => noteData.scoringType == ScoringType.BurstSliderElement;
		private bool IsBomb(ScoringElement scoringElement) => IsBomb(scoringElement.noteData);
		private bool IsBomb(NoteData noteData) => noteData.gameplayType == GameplayType.Bomb;
		private bool IsCutBad(ScoringElement scoringElement) => scoringElement is BadCutScoringElement;
		private bool IsCutBad(NoteCutInfo noteCutInfo) => !noteCutInfo.allIsOK;
		private bool IsCutUnableToExceedThreshold(GoodCutScoringElement scoringElement) => MaxPotentialScore(scoringElement) < config.GetThreshold(scoringElement.noteData.scoringType);
		private bool IsCutAboveThreshold(GoodCutScoringElement goodCutScoringElement) => IsCutAboveThreshold((CutScoreBuffer)goodCutScoringElement.cutScoreBuffer);
		private bool IsCutAboveThreshold(CutScoreBuffer cutScoreBuffer) => cutScoreBuffer.cutScore >= config.GetThreshold(cutScoreBuffer.noteCutInfo.noteData.scoringType);

		private int MaxPotentialScore(GoodCutScoringElement scoringElement) => scoringElement.maxPossibleCutScore - MissedCenterDistanceCutScore(scoringElement);
		private int MissedCenterDistanceCutScore(GoodCutScoringElement scoringElement) => scoringElement.cutScoreBuffer.noteScoreDefinition.maxCenterDistanceCutScore - scoringElement.cutScoreBuffer.centerDistanceCutScore;
	}
}