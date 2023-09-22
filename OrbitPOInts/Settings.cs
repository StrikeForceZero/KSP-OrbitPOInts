namespace OrbitPOInts
{
    // TODO: this is lazy, come up with a better way later
    public static class Settings
    {
        private static bool _globalEnable = true;

        public static bool GlobalEnable
        {
            get => _globalEnable;
            set
            {
                if (_globalEnable == value) return;
                _globalEnable = value;
                OrbitPoiVisualizer.Instance.SetEnabled(value);
                OrbitPoiVisualizer.Instance.enabled = value;
                if (OrbitPoiVisualizer.Instance.enabled)
                {
                    OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
                }
                else
                {
                    OrbitPoiVisualizer.Instance.RemoveAll();
                }
                // TODO: maybe we should clean up and remove events too
            }
        }

        private static bool _enableSpheres = true;

        public static bool EnableSpheres
        {
            get => _enableSpheres;
            set
            {
                if (_enableSpheres == value) return;
                _enableSpheres = value;
                OrbitPoiVisualizer.Instance.DrawSpheres = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _alignSpheres = false;

        public static bool AlignSpheres
        {
            get => _alignSpheres;
            set
            {
                if (_alignSpheres == value) return;
                _alignSpheres = value;
                OrbitPoiVisualizer.Instance.AlignSpheres = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _enableCircles = true;

        public static bool EnableCircles
        {
            get => _enableCircles;
            set
            {
                if (_enableCircles == value) return;
                _enableCircles = value;
                OrbitPoiVisualizer.Instance.DrawCircles = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _enablePOI_HillSphere = false;

        public static bool EnablePOI_HillSphere
        {
            get => _enablePOI_HillSphere;
            set
            {
                if (_enablePOI_HillSphere == value) return;
                _enablePOI_HillSphere = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _enablePOI_SOI = true;

        public static bool EnablePOI_SOI
        {
            get => _enablePOI_SOI;
            set
            {
                if (_enablePOI_SOI == value) return;
                _enablePOI_SOI = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _enablePOI_Atmo = true;

        public static bool EnablePOI_Atmo
        {
            get => _enablePOI_Atmo;
            set
            {
                if (_enablePOI_Atmo == value) return;
                _enablePOI_Atmo = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _enablePOI_MinOrbit = true;

        public static bool EnablePOI_MinOrbit
        {
            get => _enablePOI_MinOrbit;
            set
            {
                if (_enablePOI_MinOrbit == value) return;
                _enablePOI_MinOrbit = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _enablePOI_MaxAlt = true;

        public static bool EnablePOI_MaxAlt
        {
            get => _enablePOI_MaxAlt;
            set
            {
                if (_enablePOI_MaxAlt == value) return;
                _enablePOI_MaxAlt = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        private static bool _showPoiMaxAltOnAtmoBodies = false;

        public static bool ShowPOI_MaxAlt_OnAtmoBodies
        {
            get => _showPoiMaxAltOnAtmoBodies;
            set
            {
                if (_showPoiMaxAltOnAtmoBodies == value) return;
                _showPoiMaxAltOnAtmoBodies = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }
    }
}
