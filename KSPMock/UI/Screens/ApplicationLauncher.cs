using System;
using UnityEngineMock;

namespace KSPMock.UI.Screens
{
    public class ApplicationLauncher
    {
        public static ApplicationLauncher Instance { get; private set; }

        public ApplicationLauncherButton AddModApplication(
            Callback onTrue,
            Callback onFalse,
            Callback onHover,
            Callback onHoverOut,
            Callback onEnable,
            Callback onDisable,
            ApplicationLauncher.AppScenes visibleInScenes,
            Texture texture
        )
        {
            throw new NotImplementedException();
        }

        public void RemoveModApplication(ApplicationLauncherButton button)
        {
            throw new NotImplementedException();
        }

        [Flags]
        public enum AppScenes
        {
            NEVER = 0,
            ALWAYS = -1, // 0xFFFFFFFF
            SPACECENTER = 1,
            FLIGHT = 2,
            MAPVIEW = 4,
            VAB = 8,
            SPH = 16, // 0x00000010
            TRACKSTATION = 32, // 0x00000020
            MAINMENU = 64, // 0x00000040
        }
    }

}
