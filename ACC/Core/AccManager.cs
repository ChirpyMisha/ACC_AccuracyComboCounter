#nullable enable
using System;
using ACC.Configuration;
using Zenject;

namespace ACC.Core
{
	public class AccManager : IInitializable, IDisposable
	{
		public event EventHandler? OnComboUpdate;
		public event EventHandler? OnComboBroken;

		PluginConfig config;

		private int provisionalCutCount;
		public int NoteCount { get; private set; }
		public int Combo { get; private set; }
		public int MaxCombo { get; private set; }
		public int Misses { get; private set; }
		public int ProvisionalCombo => Combo + provisionalCutCount;
		public int CutCount => NoteCount - Misses;

		public AccManager() => config = PluginConfig.Instance;

		public void Initialize()
		{
			Reset();
		}
		public void Dispose()
		{
			return;
		}

		public void Reset()
		{
			NoteCount = 0;
			provisionalCutCount = 0;
			Combo = 0;
			MaxCombo = 0;
			Misses = 0;
		}

		public void IncreaseCombo(IncreaseComboType type)
		{
			//Plugin.Log.Notice("IncreaseCombo of type" + type.ToString());

			if (type == IncreaseComboType.ProvisionalOnCut)
				provisionalCutCount++;
			else if (type == IncreaseComboType.ProvisionalFinish)
				provisionalCutCount--;

			if (type == IncreaseComboType.OnCut || type == IncreaseComboType.ProvisionalFinish)
				IncreaseCombo();

			// Inform listeners that the combo has updated
			InvokeComboUpdate();
		}

		private void IncreaseCombo()
		{
			NoteCount++;
			if (++Combo > MaxCombo)
				MaxCombo = Combo;
			//Plugin.Log.Notice($"Combo Increased. [combo: {Combo}, MaxCombo: {MaxCombo}, ProvisionalCount: {provisionalCutCount}, Misses: {Misses}]");
		}

		public void BreakCombo(BrokenComboType type)
		{
			if (!IsTurnedOffInSettings(type))
			{
				if (type == BrokenComboType.BelowThresholdOnFinish)
					provisionalCutCount--;

				BreakCombo();
			}
		}

		private bool IsTurnedOffInSettings(BrokenComboType type)
		{
			if (type == BrokenComboType.BelowThresholdOnCut || type == BrokenComboType.BelowThresholdOnFinish)
				return false;
			else if (type == BrokenComboType.BadCut && config.BreakOnBadCut)
				return false;
			else if (type == BrokenComboType.Miss && config.BreakOnMiss)
				return false;
			else if (type == BrokenComboType.BombCut && config.BreakOnBomb)
				return false;
			else if (type == BrokenComboType.HeadWasInObstacle && config.BreakOnWall)
				return false;
			return true;
		}

		private void BreakCombo()
		{
			NoteCount++;
			Misses++;
			Combo = 0;

			// Inform listeners that the combo has updated
			InvokeComboBroken();
			InvokeComboUpdate();
		}


		protected virtual void InvokeComboUpdate()
		{
			// Create event handler
			EventHandler? handler = OnComboUpdate;
			if (handler != null)
			{
				// Invoke event
				handler(this, EventArgs.Empty);
			}
		}

		protected virtual void InvokeComboBroken()
		{
			// Create event handler
			EventHandler? handler = OnComboBroken;
			if (handler != null)
			{
				// Invoke event
				handler(this, EventArgs.Empty);
			}
		}
	}

	public enum BrokenComboType
	{
		Miss,
		BadCut,
		BombCut,
		BelowThresholdOnCut,
		BelowThresholdOnFinish,
		HeadWasInObstacle
	}

	public enum IncreaseComboType
	{
		OnCut,
		ProvisionalOnCut,
		ProvisionalFinish
	}
}