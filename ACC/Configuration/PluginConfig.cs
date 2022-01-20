using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ACC.Configuration
{
	internal class PluginConfig
	{
		public static PluginConfig Instance { get; set; } = null!;

		public virtual int AccuracyThreshold { get; set; } = 1;
		public virtual bool ShowOnResultsScreen { get; set; } = true;
		public ExtraCounterPositions MaxComboPosition { get; set; } = ExtraCounterPositions.BelowCounterPosOne;
		public ExtraCounterPositions LowAccCutsPosition { get; set; } = ExtraCounterPositions.BelowCounterPosTwo;
		public virtual bool BreakOnMiss { get; set; } = true;
		public virtual bool BreakOnBadCut { get; set; } = true;
		public virtual bool BreakOnBomb { get; set; } = true;
		public virtual bool BreakOnWall { get; set; } = true;
		public virtual bool HideComboBreakAnimation { get; set; } = false;
		public virtual string ComboLabelText { get; set; } = "Combo > %t";
		public virtual string ComboCounterText { get; set; } = "%c";
		public virtual string MaxComboCounterText { get; set; } = "Max Combo : %m";
		public virtual string LowAccCutsCounterText { get; set; } = "Cuts Below %t : %l";


		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload()
		{
			// Do stuff after config is read from disk.
		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed()
		{
			// Do stuff when the config is changed.
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(PluginConfig other)
		{
			// This instance's members populated from other
		}
	}
	public enum ExtraCounterPositions
	{
		AboveCounterPosTwo,
		AboveCounterPosOne,
		Disabled,
		BelowCounterPosOne,
		BelowCounterPosTwo
	}
}