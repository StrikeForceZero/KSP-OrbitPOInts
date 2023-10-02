using System;

namespace KSPMock
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KSPAddon : Attribute
    {
        public KSPAddon.Startup startup;
        public bool once;

        public KSPAddon(KSPAddon.Startup startup, bool once)
        {
            this.startup = startup;
            this.once = once;
        }

        public enum Startup
        {
            FlightEditorAndKSC = -6, // 0xFFFFFFFA
            AllGameScenes = -5, // 0xFFFFFFFB
            FlightAndEditor = -4, // 0xFFFFFFFC
            FlightAndKSC = -3, // 0xFFFFFFFD
            Instantly = -2, // 0xFFFFFFFE
            EveryScene = -1, // 0xFFFFFFFF
            MainMenu = 2,
            Settings = 3,
            Credits = 4,
            SpaceCentre = 5,
            EditorAny = 6,
            EditorSPH = 6,
            EditorVAB = 6,
            Flight = 7,
            TrackingStation = 8,
            PSystemSpawn = 9,
        }
    }
}
