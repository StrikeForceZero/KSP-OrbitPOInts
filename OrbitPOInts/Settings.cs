using OrbitPOInts.Extensions;

namespace OrbitPOInts
{
    // TODO: this is lazy, come up with a better way later
    public static class Settings
    {
        private static bool _globalEnable = true;

        private static bool _activeBodyOnly = true;

        private static bool _enableSpheres = true;
        private static bool _alignSpheres;
        private static bool _enableCircles = true;

        private static bool _enablePOI_HillSphere;
        private static bool _enablePOI_SOI = true;
        private static bool _enablePOI_Atmo = true;
        private static bool _enablePOI_MinOrbit = true;
        private static bool _enablePOI_MaxAlt = true;
        private static bool _showPoiMaxAltOnAtmoBodies;

        private static bool _customPoisFromCenter;
        private static double _customPoi1;
        private static bool _customPoi1Enabled;
        private static double _customPoi2;
        private static bool _customPoi2Enabled;
        private static double _customPoi3;
        private static bool _customPoi3Enabled;

        public static bool GlobalEnable
        {
            get => _globalEnable;
            set
            {
                if (_globalEnable == value) return;
                _globalEnable = value;
                OrbitPoiVisualizer.Instance.enabled = value;
                if (OrbitPoiVisualizer.Instance.enabled) OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool ActiveBodyOnly
        {
            get => _activeBodyOnly;
            set
            {
                if (_activeBodyOnly == value) return;
                _activeBodyOnly = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool EnableSpheres
        {
            get => _enableSpheres;
            set
            {
                if (_enableSpheres == value) return;
                _enableSpheres = value;
                OrbitPoiVisualizer.Instance.DrawSpheres = value;
                OrbitPoiVisualizer.Instance.PurgeSpheres();
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

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

        public static bool EnableCircles
        {
            get => _enableCircles;
            set
            {
                if (_enableCircles == value) return;
                _enableCircles = value;
                OrbitPoiVisualizer.Instance.DrawCircles = value;
                OrbitPoiVisualizer.Instance.PurgeCircles();
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

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

        public static bool CustomPOiFromCenter
        {
            get => _customPoisFromCenter;
            set
            {
                if (_customPoisFromCenter == value) return;
                _customPoisFromCenter = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static double CustomPOI1
        {
            get => _customPoi1;
            set
            {
                if (_customPoi1.AreRelativelyEqual(value)) return;
                _customPoi1 = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool CustomPOI1Enabled
        {
            get => _customPoi1Enabled;
            set
            {
                if (_customPoi1Enabled == value) return;
                _customPoi1Enabled = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static double CustomPOI2
        {
            get => _customPoi2;
            set
            {
                if (_customPoi2.AreRelativelyEqual(value)) return;
                _customPoi2 = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool CustomPOI2Enabled
        {
            get => _customPoi2Enabled;
            set
            {
                if (_customPoi2Enabled == value) return;
                _customPoi2Enabled = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static double CustomPOI3
        {
            get => _customPoi3;
            set
            {
                if (_customPoi3.AreRelativelyEqual(value)) return;
                _customPoi3 = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool CustomPOI3Enabled
        {
            get => _customPoi3Enabled;
            set
            {
                if (_customPoi3Enabled == value) return;
                _customPoi3Enabled = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }
    }
}
