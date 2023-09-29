using System;
using OrbitPOInts.Data;
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
            Log("[OnLoad] loading settings");
            base.OnLoad(node);
            // TODO: finish
            var loadedVersion = node.GetUInt(nameof(Settings.VERSION));
            if (loadedVersion < Settings.VERSION)
            {
                Logger.LogError("Older settings version!");
                // TODO: converters
            }
            Settings.GlobalEnable = node.GetBool(GetKey(SettingsBool.GlobalEnable), true);
            Settings.FocusedBodyOnly = node.GetBool(GetKey(SettingsBool.FocusedBodyOnly), true);
            Settings.EnableSpheres = node.GetBool(GetKey(SettingsBool.EnableSpheres), true);
            Settings.AlignSpheres = node.GetBool(GetKey(SettingsBool.AlignSpheres), false);
            Settings.EnableCircles = node.GetBool(GetKey(SettingsBool.EnableCircles), true);
            Settings.LogDebugEnabled = node.GetBool(GetKey(SettingsBool.LogDebugEnabled), false);
            Settings.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies = node.GetBool(GetKey(SettingsBool.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies), false);

            var poiNodes  = node.GetNodes(PoiConfigKey);
            Settings.UpdateConfiguredPois(poiNodes.Select(poiNode => PoiDTO.Load(poiNode).ToPoi()).ToList());
            Log("[OnLoad] load complete");
        }

        public override void OnSave(ConfigNode node)
        {
            Log("[OnSave] saving settings");
            base.OnSave(node);
            node.AddValue(nameof(Settings.VERSION), Settings.VERSION);
            node.AddValue(GetKey(SettingsBool.GlobalEnable), Settings.GlobalEnable);
            node.AddValue(GetKey(SettingsBool.FocusedBodyOnly), Settings.FocusedBodyOnly);
            node.AddValue(GetKey(SettingsBool.EnableSpheres), Settings.EnableSpheres);
            node.AddValue(GetKey(SettingsBool.AlignSpheres), Settings.AlignSpheres);
            node.AddValue(GetKey(SettingsBool.EnableCircles), Settings.EnableCircles);
            node.AddValue(GetKey(SettingsBool.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies), Settings.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies);
            node.AddValue(GetKey(SettingsBool.LogDebugEnabled), Settings.LogDebugEnabled);

            foreach (var poiConfigNode in Settings.ConfiguredPois.Select(poi => PoiDTO.FromPoi(poi).Save()))
            {
                node.AddNode(PoiConfigKey, poiConfigNode);
            }
            Log("[OnSave] saving complete");
        }

        public void Start()
        {
            Instance = this;
        }
    }
}
