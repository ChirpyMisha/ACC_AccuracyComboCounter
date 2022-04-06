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

		private bool isBreakingHighscore;
		private int provisionalCutCount;
		public int NoteCount { get; private set; }
		public int Combo { get; private set; }
		public int MaxCombo { get; private set; }
		public int LowAccCuts { get; private set; }
		public int ProvisionalCombo => Combo + provisionalCutCount;
		public int ProvisionalMaxCombo => isBreakingHighscore ? MaxCombo + provisionalCutCount : MaxCombo;
		public int HighAccCuts => NoteCount - LowAccCuts;

		public string InsertValuesInFormattedString(string s)
		{
			for (int i = 0; i < s.Length-1; i++)
			{
				if (s[i] == '%')
				{
					switch (char.ToLower(s[i + 1]))
					{
						
						case 'c':
							int combo = config.AssumeFullSwingScoreOnCut ? ProvisionalCombo : Combo;
							s = InsertValAt(s, combo, i);
							break;
						case 'm':
							int maxCombo = config.AssumeFullSwingScoreOnCut ? ProvisionalMaxCombo : MaxCombo;
							s = InsertValAt(s, maxCombo, i);
							break;
						case 'l':
							s = InsertValAt(s, LowAccCuts, i);
							break;
						case 't':
							s = InsertValAt(s, config.AccuracyThresholdNormal, i);
							break;
						case 'n':
							s = InsertValAt(s, NoteCount, i);
							break;
						case 'h':
							s = InsertValAt(s, HighAccCuts, i);
							break;
					}
				}
			}

			return s;
		}
		private string InsertValAt(string str, int val, int i)
		{
			str = str.Remove(i, 2);
			str = str.Insert(i, val.ToString());
			return str;
		}

		public AccManager() { config = PluginConfig.Instance; Plugin.AccManager = this; }

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
			LowAccCuts = 0;
			isBreakingHighscore = true;
		}

		public void IncreaseCombo(IncreaseComboType type)
		{
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
			{
				MaxCombo = Combo;
				isBreakingHighscore = true;
			}
		}

		public void BreakCombo(BrokenComboType type)
		{
			Plugin.Log.Notice($"BreakCombo: Type = {type}");
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
			else if (type == BrokenComboType.HeadIsInObstacle && config.BreakOnWall)
				return false;
			return true;
		}

		private void BreakCombo()
		{
			NoteCount++;
			LowAccCuts++;
			Combo = 0;
			isBreakingHighscore = false;

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

		internal string InsertValuesInFormattedString(object maxComboCounterText)
		{
			throw new NotImplementedException();
		}
	}

	public enum BrokenComboType
	{
		Miss,
		BadCut,
		BombCut,
		BelowThresholdOnCut,
		BelowThresholdOnFinish,
		HeadIsInObstacle
	}

	public enum IncreaseComboType
	{
		OnCut,
		ProvisionalOnCut,
		ProvisionalFinish
	}
}