using System;

namespace KSPMock
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class KSPScenario : Attribute
    {
        public ScenarioCreationOptions createOptions;
        public GameScenes[] tgtScenes;

        public KSPScenario(ScenarioCreationOptions createOptions, params GameScenes[] tgtScenes)
        {
            this.createOptions = createOptions;
            this.tgtScenes = tgtScenes;
        }

        public bool HasCreateOption(ScenarioCreationOptions option) => createOptions != null && option != null;

        public GameScenes[] TargetScenes => this.tgtScenes;

        public bool HasTargetScene(GameScenes scene)
        {
            throw new NotImplementedException();
        }
    }
}
