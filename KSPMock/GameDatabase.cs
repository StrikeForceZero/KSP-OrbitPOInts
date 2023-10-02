using System;
using UnityEngineMock;

namespace KSPMock
{
    public class GameDatabase
    {
        public static GameDatabase Instance { get; set; }

        public Texture GetTexture(string path, bool asNormalMaps) { throw new NotImplementedException(); }
    }
}
