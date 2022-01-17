using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
using System.Linq;
//using static ACC.Configuration.PluginConfig;

namespace ACC.Configuration
{
	class ConfigController
	{
		[UIValue("AccuracyThreshold")]
		public virtual int AccuracyThreshold
		{
			get { return PluginConfig.Instance.AccuracyThreshold; }
			set { PluginConfig.Instance.AccuracyThreshold = value; }
		}

		[UIValue("ShowOnResultsScreen")]
		public virtual bool ShowOnResultsScreen
		{
			get { return PluginConfig.Instance.ShowOnResultsScreen; }
			set { PluginConfig.Instance.ShowOnResultsScreen = value; }
		}

		[UIValue("MaxComboPosition")]
		public virtual ExtraCounterPositions MaxComboPosition
		{
			get { return PluginConfig.Instance.MaxComboPosition; }
			set { PluginConfig.Instance.MaxComboPosition = value; }
		}

		[UIValue("LowAccCutsPosition")]
		public virtual ExtraCounterPositions LowAccCutsPosition
		{
			get { return PluginConfig.Instance.LowAccCutsPosition; }
			set { PluginConfig.Instance.LowAccCutsPosition = value; }
		}

		[UIValue("BreakOnMiss")]
		public virtual bool BreakOnMiss
		{
			get { return PluginConfig.Instance.BreakOnMiss; }
			set { PluginConfig.Instance.BreakOnMiss = value; }
		}

		[UIValue("BreakOnBadCut")]
		public virtual bool BreakOnBadCut
		{
			get { return PluginConfig.Instance.BreakOnBadCut; }
			set { PluginConfig.Instance.BreakOnBadCut = value; }
		}

		[UIValue("BreakOnBomb")]
		public virtual bool BreakOnBomb
		{
			get { return PluginConfig.Instance.BreakOnBomb; }
			set { PluginConfig.Instance.BreakOnBomb = value; }
		}

		[UIValue("BreakOnWall")]
		public virtual bool BreakOnWall
		{
			get { return PluginConfig.Instance.BreakOnWall; }
			set { PluginConfig.Instance.BreakOnWall = value; }
		}

		[UIValue("HideComboBreakAnimation")]
		public virtual bool HideComboBreakAnimation
		{
			get { return PluginConfig.Instance.HideComboBreakAnimation; }
			set { PluginConfig.Instance.HideComboBreakAnimation = value; }
		}



		[UIValue(nameof(ExtraCounterPositionsList))]
		public List<object> ExtraCounterPositionsList => ExtraCounterPositionsToNames.Keys.Cast<object>().ToList();

		[UIAction(nameof(ExtraCounterPositionsFormat))]
		public string ExtraCounterPositionsFormat(ExtraCounterPositions position) => ExtraCounterPositionsToNames[position];

		private static Dictionary<ExtraCounterPositions, string> ExtraCounterPositionsToNames = new Dictionary<ExtraCounterPositions, string>()
		{
			{ExtraCounterPositions.AboveCounterPosTwo, "Above +2" },
			{ExtraCounterPositions.AboveCounterPosOne, "Above +1" },
			{ExtraCounterPositions.Disabled, "Disabled" },
			{ExtraCounterPositions.BelowCounterPosOne, "Below -1" },
			{ExtraCounterPositions.BelowCounterPosTwo, "Below -2" }
		};
	}
}
