namespace OrbitPOInts.UI
{
    sealed class ToggleTextFieldResult
    {
        public bool Enabled;
        public string Text;

        public static ToggleTextFieldResult Default => new ToggleTextFieldResult();
    }
}
