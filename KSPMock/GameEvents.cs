namespace KSPMock
{
    public class GameEvents : GameEventsBase
    {
        public struct HostedFromToAction<A, B>
        {
            public A host;
            public B from;
            public B to;

            public HostedFromToAction(A host, B from, B to)
            {
                this.host = host;
                this.from = from;
                this.to = to;
            }
        }

        public static EventVoid OnMapEntered = new EventVoid(nameof (OnMapEntered));
        public static EventVoid OnMapExited = new EventVoid(nameof (OnMapExited));
        public static EventData<MapObject> OnMapFocusChange = new EventData<MapObject>(nameof (OnMapFocusChange));
        public static EventData<Vessel> onVesselChange = new EventData<Vessel>(nameof (onVesselChange));
        public static EventData<GameEvents.HostedFromToAction<Vessel, CelestialBody>> onVesselSOIChanged = new EventData<GameEvents.HostedFromToAction<Vessel, CelestialBody>>("OnVesselSOIChanged");
        public static EventData<GameScenes> onGameSceneLoadRequested = new EventData<GameScenes>(nameof (onGameSceneLoadRequested));
        public static EventData<GameScenes> onLevelWasLoadedGUIReady = new EventData<GameScenes>(nameof (onLevelWasLoadedGUIReady));
    }
}
