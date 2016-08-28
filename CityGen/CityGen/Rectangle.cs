namespace CityGen
{
    public class Rectangle
    {
        public Vector LeftTop { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Rectangle(Vector leftTop, int width, int height)
        {
            LeftTop = leftTop;
            Width = width;
            Height = height;
        }

        public bool Intersects(Rectangle b)
        {
            return
                (LeftTop.X + Width) >= b.LeftTop.X &&
                LeftTop.X <= (b.LeftTop.X + b.Width) &&
                (LeftTop.Y + Height) >= b.LeftTop.Y &&
                LeftTop.Y <= (b.LeftTop.Y + b.Height);
        }
    }
}
