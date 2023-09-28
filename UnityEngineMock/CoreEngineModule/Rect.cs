using System;

namespace UnityEngineMock
{
    public class Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rect(Vector2 position, Vector2 size)
        {
            this.x = position.x;
            this.y = position.y;
            this.width = size.x;
            this.height = size.y;
        }

        public Rect(Rect source)
        {
            this.x = source.x;
            this.y = source.y;
            this.width = source.width;
            this.height = source.height;
        }

        public bool Contains(Vector2 pos) { throw new NotImplementedException(); }
        public static Rect zero => new Rect(0.0f, 0.0f, 0.0f, 0.0f);
    }
}
