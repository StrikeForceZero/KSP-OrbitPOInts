using System;

namespace KSPMock.UI.Screens
{
    public class ApplicationLauncherButton
    {
        public void SetFalse() { throw new NotImplementedException(); }

        public static bool operator true(ApplicationLauncherButton button)
        {
            return button != null;
        }

        public static bool operator false(ApplicationLauncherButton button)
        {
            return button == null;
        }

        public static implicit operator bool(ApplicationLauncherButton exists) => exists != null;
    }
}
