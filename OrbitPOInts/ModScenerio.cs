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
            Settings.GlobalEnable = node.GetBool(GetKey(SettingsBool.GlobalEnable), true);
            Settings.ActiveBodyOnly = node.GetBool(GetKey(SettingsBool.ActiveBodyOnly), true);
            Settings.EnableSpheres = node.GetBool(GetKey(SettingsBool.EnableSpheres), true);
            Settings.AlignSpheres = node.GetBool(GetKey(SettingsBool.AlignSpheres), false);
            Settings.EnableCircles = node.GetBool(GetKey(SettingsBool.EnableCircles), true);
            Settings.EnablePOI_HillSphere = node.GetBool(GetKey(SettingsBool.EnablePOI_HillSphere), false);
            Settings.EnablePOI_SOI = node.GetBool(GetKey(SettingsBool.EnablePOI_SOI), true);
            Settings.EnablePOI_Atmo = node.GetBool(GetKey(SettingsBool.EnablePOI_Atmo), true);
            Settings.EnablePOI_MinOrbit = node.GetBool(GetKey(SettingsBool.EnablePOI_MinOrbit), true);
            Settings.EnablePOI_MaxAlt = node.GetBool(GetKey(SettingsBool.EnablePOI_MaxAlt), true);
            Settings.ShowPOI_MaxAlt_OnAtmoBodies = node.GetBool(GetKey(SettingsBool.ShowPOI_MaxAlt_OnAtmoBodies), false);
            Settings.CustomPOiFromCenter = node.GetBool(GetKey(SettingsBool.CustomPOiFromCenter), false);
            Settings.LogDebugEnabled = node.GetBool(GetKey(SettingsBool.LogDebugEnabled), false);
            Settings.CustomPOI1Enabled = node.GetBool(GetKey(SettingsBool.CustomPOI1Enabled), false);
            Settings.CustomPOI2Enabled = node.GetBool(GetKey(SettingsBool.CustomPOI2Enabled), false);
            Settings.CustomPOI3Enabled = node.GetBool(GetKey(SettingsBool.CustomPOI3Enabled), false);

            Settings.CustomPOI1 = node.GetDouble(GetKey(SettingsDouble.CustomPOI1), 0.0);
            Settings.CustomPOI2 = node.GetDouble(GetKey(SettingsDouble.CustomPOI2), 0.0);
            Settings.CustomPOI3 = node.GetDouble(GetKey(SettingsDouble.CustomPOI3), 0.0);

            foreach (PoiType poiType in Enum.GetValues(typeof(PoiType)))
            {
                var serializedColor = node.GetString(GetKey(SettingsDictionary.PoiColors) + "_" + poiType, null);
                if (serializedColor != null && ColorExtensions.TryDeserialize(serializedColor, out var color))
                {
                    Settings.PoiColors[poiType] = color;
                }
            }
            Log("[OnLoad] load complete");
        }

        public override void OnSave(ConfigNode node)
        {
            Log("[OnSave] saving settings");
            base.OnSave(node);
            node.AddValue(GetKey(SettingsBool.GlobalEnable), Settings.GlobalEnable);
            node.AddValue(GetKey(SettingsBool.ActiveBodyOnly), Settings.ActiveBodyOnly);
            node.AddValue(GetKey(SettingsBool.EnableSpheres), Settings.EnableSpheres);
            node.AddValue(GetKey(SettingsBool.AlignSpheres), Settings.AlignSpheres);
            node.AddValue(GetKey(SettingsBool.EnableCircles), Settings.EnableCircles);
            node.AddValue(GetKey(SettingsBool.EnablePOI_HillSphere), Settings.EnablePOI_HillSphere);
            node.AddValue(GetKey(SettingsBool.EnablePOI_SOI), Settings.EnablePOI_SOI);
            node.AddValue(GetKey(SettingsBool.EnablePOI_Atmo), Settings.EnablePOI_Atmo);
            node.AddValue(GetKey(SettingsBool.EnablePOI_MinOrbit), Settings.EnablePOI_MinOrbit);
            node.AddValue(GetKey(SettingsBool.EnablePOI_MaxAlt), Settings.EnablePOI_MaxAlt);
            node.AddValue(GetKey(SettingsBool.ShowPOI_MaxAlt_OnAtmoBodies), Settings.ShowPOI_MaxAlt_OnAtmoBodies);
            node.AddValue(GetKey(SettingsBool.CustomPOiFromCenter), Settings.CustomPOiFromCenter);
            node.AddValue(GetKey(SettingsBool.LogDebugEnabled), Settings.LogDebugEnabled);
            node.AddValue(GetKey(SettingsBool.CustomPOI1Enabled), Settings.CustomPOI1Enabled);
            node.AddValue(GetKey(SettingsBool.CustomPOI2Enabled), Settings.CustomPOI2Enabled);
            node.AddValue(GetKey(SettingsBool.CustomPOI3Enabled), Settings.CustomPOI3Enabled);

            node.AddValue(GetKey(SettingsDouble.CustomPOI1), Settings.CustomPOI1);
            node.AddValue(GetKey(SettingsDouble.CustomPOI2), Settings.CustomPOI2);
            node.AddValue(GetKey(SettingsDouble.CustomPOI3), Settings.CustomPOI3);

            foreach (var entry in Settings.PoiColors)
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
