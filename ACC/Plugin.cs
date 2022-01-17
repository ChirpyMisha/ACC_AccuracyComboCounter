using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using System.Reflection;
using ACC.Core;
using ACC.ResultsView;
using Zenject;
using IPALogger = IPA.Logging.Logger;
using ACC.Configuration;

namespace ACC
{
	[Plugin(RuntimeOptions.DynamicInit)]
	public class Plugin
	{
#pragma warning disable CS8618
		public static Harmony harmony;

		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }
#pragma warning restore CS8618

		[Init]
		/// <summary>
		/// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
		/// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
		/// Only use [Init] with one Constructor.
		/// </summary>
		public Plugin(IPALogger logger, Zenjector zenjector)
		{
			Instance = this;
			Plugin.Log = logger;

			zenjector.Install(Location.App, (DiContainer Container) =>
			{
				Container.BindInterfacesAndSelfTo<AccManager>().AsSingle();
			});
			zenjector.Install(Location.GameCore, (DiContainer Container) =>
			{
				Container.BindInterfacesAndSelfTo<AccTracker>().AsSingle();
			});
			zenjector.Install(Location.Menu, (DiContainer Container) =>
			{
				Container.BindInterfacesAndSelfTo<AccResultsViewController>().AsSingle();
			});
		}

		#region BSIPA Config
		//Uncomment to use BSIPA's config
		[Init]
		public void InitWithConfig(Config conf)
		{
			PluginConfig.Instance = conf.Generated<PluginConfig>();
			Plugin.Log?.Debug("Config loaded");
		}
		#endregion


		#region Disableable

		[OnEnable]
		public void OnApplicationStart()
		{
			harmony = new Harmony("com.ChirpyMisha.BeatSaber.ACC");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		[OnDisable]
		public void OnApplicationQuit()
		{
			harmony.UnpatchSelf();
		}

		/*
        /// <summary>
        /// Called when the plugin is disabled and on Beat Saber quit.
        /// Return Task for when the plugin needs to do some long-running, asynchronous work to disable.
        /// [OnDisable] methods that return Task are called after all [OnDisable] methods that return void.
        /// </summary>
        [OnDisable]
        public async Task OnDisableAsync()
        {
            await LongRunningUnloadTask().ConfigureAwait(false);
        }
        */
		#endregion

		// Uncomment the methods in this section if using Harmony
		#region Harmony
		/*
        /// <summary>
        /// Attempts to apply all the Harmony patches in this assembly.
        /// </summary>
        internal static void ApplyHarmonyPatches()
        {
            try
            {
                Plugin.Log?.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error("Error applying Harmony patches: " + ex.Message);
                Plugin.Log?.Debug(ex);
            }
        }

        /// <summary>
        /// Attempts to remove all the Harmony patches that used our HarmonyId.
        /// </summary>
        internal static void RemoveHarmonyPatches()
        {
            try
            {
                // Removes all patches with this HarmonyId
                harmony.UnpatchAll(HarmonyId);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error("Error removing Harmony patches: " + ex.Message);
                Plugin.Log?.Debug(ex);
            }
        }
        */
		#endregion
	}
}
