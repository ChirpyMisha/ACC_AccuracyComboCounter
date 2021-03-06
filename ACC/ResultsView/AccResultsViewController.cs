#nullable enable
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Utilities;
using System;
using System.Reflection;
using ACC.Core;
using ACC.Configuration;
using TMPro;
using Zenject;

namespace ACC.ResultsView
{
	class AccResultsViewController : IInitializable, IDisposable
	{
		private static readonly string ResourceNameAccThreshold = "ACC.ResultsView.UI.Views.ResultsComboResult.bsml";

		private static FieldAccessor<ResultsViewController, LevelCompletionResults>.Accessor LevelCompletionResults = FieldAccessor<ResultsViewController, LevelCompletionResults>.GetAccessor("_levelCompletionResults");

		// Text fields in the bsml
		[UIComponent("AccResultText")]
		private TextMeshProUGUI? accResultText = null!;
		[UIComponent("AccMaxComboText")]
		private TextMeshProUGUI? accMaxComboText = null!;

		private readonly AccManager accManager;
		private ResultsViewController resultsViewController;
		private LevelCompletionResults levelCompletionResults = null!;
		private readonly PluginConfig config;

		public AccResultsViewController(AccManager accManager, ResultsViewController resultsViewController)
		{
			this.accManager = accManager;
			this.resultsViewController = resultsViewController;
			this.config = PluginConfig.Instance;
		}

		public void Initialize()
		{
			if (resultsViewController != null)
				resultsViewController.didActivateEvent += ResultsViewController_OnActivateEvent;
		}

		public void Dispose()
		{
			if (resultsViewController != null)
				resultsViewController.didActivateEvent -= ResultsViewController_OnActivateEvent;
		}

		private void ResultsViewController_OnActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
		{
			levelCompletionResults = LevelCompletionResults(ref resultsViewController);

			ParseAllBSML();

			if (levelCompletionResults.levelEndStateType == global::LevelCompletionResults.LevelEndStateType.Cleared)
				SetResultsViewText();
			else
				EmptyResultsViewText();
		}

		private void ParseAllBSML()
		{
			if (accResultText == null || accMaxComboText == null)
			{
				ParseBSML(ResourceNameAccThreshold);

				if (accResultText != null && accMaxComboText != null)
				{
					accResultText.fontSize *= 1.2f;
					accMaxComboText.fontSize *= 0.60f;
				}
				else
					Plugin.Log.Error($"Parsing BSML ({ResourceNameAccThreshold}) has failed.");
			}
		}

		private void ParseBSML(string bsmlPath)
		{
			BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), bsmlPath), resultsViewController.gameObject, this);
		}

		private void SetResultsViewText()
		{
			// Empty the text fields so they can be filled with new information
			EmptyResultsViewText();

			// Set the text
			if (PluginConfig.Instance.ShowOnResultsScreen && accResultText != null && accMaxComboText != null)
			{
				accResultText.text = accManager.InsertValuesInFormattedString(config.ResultText);
				string maxComboText = accManager.LowAccCuts > 0 ? config.MaxComboText : config.FullComboText;
				accMaxComboText.text = accManager.InsertValuesInFormattedString(maxComboText);
			}
		}

		private void EmptyResultsViewText()
		{
			if (accResultText != null && accMaxComboText != null)
			{
				accResultText.text = "";
				accMaxComboText.text = "";
			}
		}
	}
}