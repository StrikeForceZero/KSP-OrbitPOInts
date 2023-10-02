using System;

namespace UnityEngineMock
{
    public class Texture2D
    {
        public static extern Texture2D whiteTexture { get; set; }

        public Texture2D(int width, int height)
        {

        }

        public void SetPixels(Color[] colors)
        {
            throw new NotImplementedException();
        }

        public void Apply()
        {
            throw new NotImplementedException();
        }
    }
}
