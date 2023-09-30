using System;
using System.Collections.Generic;
using OrbitPOInts.Data;
using OrbitPOInts.Data.ConfigNode.Extensions;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;

#if TEST
using UnityEngineMock;
using KSP_KSPScenario = KSPMock.KSPScenario;
using KSP_ScenarioModule = KSPMock.ScenarioModule;
using KSP_ScenarioCreationOptions = KSPMock.ScenarioCreationOptions;
using KSP_GameScenes = KSPMock.GameScenes;
using KSP_ConfigNode = KSPMock.ConfigNode;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using KSP_KSPScenario = KSPScenario;
using KSP_ScenarioModule = ScenarioModule;
using KSP_ScenarioCreationOptions = ScenarioCreationOptions;
using KSP_GameScenes = GameScenes;
using KSP_ConfigNode = ConfigNode;
#endif

namespace OrbitPOInts
{
    using KSPScenario = KSP_KSPScenario;
    using ScenarioModule = KSP_ScenarioModule;
    using ScenarioCreationOptions = KSP_ScenarioCreationOptions;
    using GameScenes = KSP_GameScenes;
    using ConfigNode = KSP_ConfigNode;

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class ModScenario : ScenarioModule
    {
        public static ModScenario Instance { get; private set; }

        private enum SettingsBool
        {
            GlobalEnable,
            FocusedBodyOnly,
            EnableSpheres,
            AlignSpheres,
            EnableCircles,
            ShowPoiMaxTerrainAltitudeOnAtmosphericBodies,
            LogDebugEnabled,
        }

        private static string GetKey<TEnum>(TEnum key) where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), key);
        }

        private void Log(string message)
        {
            Logger.Log($"[ModScenario] {message}");
        }

        private const string PoiConfigKey = "POI";

        public override void OnLoad(ConfigNode node)
        {
            Log("[OnLoad] loading Settings.Instance");
            base.OnLoad(node);
            // TODO: finish
            var loadedVersion = node.GetUInt(nameof(Settings.VERSION));
            if (loadedVersion < Settings.VERSION)
            {
                Logger.LogError("Older Settings.Instance version!");
                // TODO: converters
            }
            Settings.Instance.GlobalEnable = node.GetBool(GetKey(SettingsBool.GlobalEnable), true);
            Settings.Instance.FocusedBodyOnly = node.GetBool(GetKey(SettingsBool.FocusedBodyOnly), true);
            Settings.Instance.EnableSpheres = node.GetBool(GetKey(SettingsBool.EnableSpheres), true);
            Settings.Instance.AlignSpheres = node.GetBool(GetKey(SettingsBool.AlignSpheres), false);
            Settings.Instance.EnableCircles = node.GetBool(GetKey(SettingsBool.EnableCircles), true);
            Settings.Instance.LogDebugEnabled = node.GetBool(GetKey(SettingsBool.LogDebugEnabled), false);
            Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies = node.GetBool(GetKey(SettingsBool.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies), false);

            Settings.Instance.UpdateConfiguredPois(LoadConfiguredPois(node));

            Log("[OnLoad] load complete");
        }

        public override void OnSave(ConfigNode node)
        {
            Log("[OnSave] saving Settings.Instance");
            base.OnSave(node);
            node.AddValue(nameof(Settings.VERSION), Settings.VERSION);
            node.AddValue(GetKey(SettingsBool.GlobalEnable), Settings.Instance.GlobalEnable);
            node.AddValue(GetKey(SettingsBool.FocusedBodyOnly), Settings.Instance.FocusedBodyOnly);
            node.AddValue(GetKey(SettingsBool.EnableSpheres), Settings.Instance.EnableSpheres);
            node.AddValue(GetKey(SettingsBool.AlignSpheres), Settings.Instance.AlignSpheres);
            node.AddValue(GetKey(SettingsBool.EnableCircles), Settings.Instance.EnableCircles);
            node.AddValue(GetKey(SettingsBool.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies), Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies);
            node.AddValue(GetKey(SettingsBool.LogDebugEnabled), Settings.Instance.LogDebugEnabled);

            SaveConfiguredPois(node, Settings.Instance.ConfiguredPois);

            Log("[OnSave] saving complete");
        }

        public static void SaveConfiguredPois(ConfigNode node, IReadOnlyList<POI> configuredPois)
        {
            foreach (var poiConfigNode in configuredPois.Select(poi => PoiDTO.FromPoi(poi).Save()))
            {
                node.AddNode(PoiConfigKey, poiConfigNode);
            }
        }

        public static IList<POI> LoadConfiguredPois(ConfigNode node)
        {
            return node.GetNodes(PoiConfigKey).Select(poiNode => PoiDTO.Load(poiNode).ToPoi()).ToList();
        }

        public void Start()
        {
            Instance = this;
        }
    }
}
