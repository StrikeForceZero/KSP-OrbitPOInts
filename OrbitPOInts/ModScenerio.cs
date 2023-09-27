using System;
using OrbitPOInts.Data;
using OrbitPOInts.Extensions;
using UnityEngine;

namespace OrbitPOInts
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class ModScenario : ScenarioModule
    {
        public static ModScenario Instance { get; private set; }

        private enum SettingsBool
        {
            GlobalEnable,
            ActiveBodyOnly,
            EnableSpheres,
            AlignSpheres,
            EnableCircles,
            EnablePOI_HillSphere,
            EnablePOI_SOI,
            EnablePOI_Atmo,
            EnablePOI_MinOrbit,
            EnablePOI_MaxAlt,
            ShowPOI_MaxAlt_OnAtmoBodies ,
            CustomPOI1Enabled,
            CustomPOI2Enabled,
            CustomPOI3Enabled,
            CustomPOiFromCenter,
            LogDebugEnabled,
        }

        private enum SettingsDouble
        {
            CustomPOI1,
            CustomPOI2,
            CustomPOI3,
        }
        
        private enum SettingsDictionary
        {
            PoiColors,
        }

        private static string GetKey<TEnum>(TEnum key) where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), key);
        }

        private void Log(string message)
        {
            Logger.Log($"[ModScenario] {message}");
        }

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
            Settings.ActiveBodyOnly = node.GetBool(GetKey(SettingsBool.ActiveBodyOnly), true);
            Settings.EnableSpheres = node.GetBool(GetKey(SettingsBool.EnableSpheres), true);
            Settings.AlignSpheres = node.GetBool(GetKey(SettingsBool.AlignSpheres), false);
            Settings.EnableCircles = node.GetBool(GetKey(SettingsBool.EnableCircles), true);
            Settings.LogDebugEnabled = node.GetBool(GetKey(SettingsBool.LogDebugEnabled), false);
            Settings.ShowPOI_MaxAlt_OnAtmoBodies = node.GetBool(GetKey(SettingsBool.ShowPOI_MaxAlt_OnAtmoBodies), false);

            foreach (PoiType poiType in Enum.GetValues(typeof(PoiType)))
            {
                var serializedColor = node.GetString(GetKey(SettingsDictionary.PoiColors) + "_" + poiType, null);
                if (serializedColor != null && ColorExtensions.TryDeserialize(serializedColor, out var color))
                {
                    Settings.FakePoiColors[poiType] = color;
                }
            }
            Log("[OnLoad] load complete");
        }

        public override void OnSave(ConfigNode node)
        {
            Log("[OnSave] saving settings");
            base.OnSave(node);
            node.AddValue(nameof(Settings.VERSION), Settings.VERSION);
            node.AddValue(GetKey(SettingsBool.GlobalEnable), Settings.GlobalEnable);
            node.AddValue(GetKey(SettingsBool.ActiveBodyOnly), Settings.ActiveBodyOnly);
            node.AddValue(GetKey(SettingsBool.EnableSpheres), Settings.EnableSpheres);
            node.AddValue(GetKey(SettingsBool.AlignSpheres), Settings.AlignSpheres);
            node.AddValue(GetKey(SettingsBool.EnableCircles), Settings.EnableCircles);
            node.AddValue(GetKey(SettingsBool.ShowPOI_MaxAlt_OnAtmoBodies), Settings.ShowPOI_MaxAlt_OnAtmoBodies);
            node.AddValue(GetKey(SettingsBool.LogDebugEnabled), Settings.LogDebugEnabled);

            foreach (var entry in Settings.FakePoiColors)
            {
                node.AddValue($"{GetKey(SettingsDictionary.PoiColors)}_{entry.Key}", entry.Value.Serialize());
            }
            Log("[OnSave] saving complete");
        }

        public void Start()
        {
            Instance = this;
        }
    }
}
