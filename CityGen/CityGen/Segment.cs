using CityGen.QuadTree;
using System;

namespace CityGen
{
    public class Segment : IBoundingBox
    {
        public Vector Start { get; private set; }
        public Vector End { get; private set; }

        public int Priority { get; private set; }
        public float Direction { get; private set; }

        public Rectangle BoundingBox
        {
            get; 
            private set;
        }

        public Segment(Vector start, Vector end, int priority)
        {
            Start = start;
            End = end;
            Priority = priority;

            var delta = End - Start;
            Direction = (float)(Math.Atan2(delta.Y, delta.X) * 180 / Math.PI) + 90f;

            BoundingBox = new Rectangle(start, delta.X, delta.Y);
        }

        public static Segment NewSegmentInDirection(Segment oldSegment, float direction, int t, int length = 5)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var start = new Vector(oldSegment.End.X, oldSegment.End.Y);
            var directionVector = oldSegment.End - oldSegment.Start;

            //start.X += directionVector.X > 0 ? 1 : 0;
            //start.Y += directionVector.Y > 0 ? 1 : 0;

            var end = new Vector(
                (int)Math.Round(start.X + length * Math.Sin((oldSegment.Direction + direction) * Math.PI / 180)),
                (int)Math.Round(start.Y + length * Math.Cos((oldSegment.Direction + direction) * Math.PI / 180))
            );

            return new Segment(start, end, oldSegment.Priority + 1 + t);
        }

        public static bool Intersect(Segment s1, Segment s2, out Vector intersection, bool considerCollinearOverlapAsIntersect = true)
        {
            intersection = new Vector();

            var r = s1.End - s1.Start;
            var s = s2.End - s2.Start;
            var rxs = r.Cross(s);
            var qpxr = (s2.Start - s1.Start).Cross(r);

            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            if (rxs == 0 && qpxr == 0)
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                if (considerCollinearOverlapAsIntersect)
                    if ((0 <= (s2.Start - s1.Start) * r && (s2.Start - s1.Start) * r <= r * r) || (0 <= (s1.Start - s2.Start) * s && (s1.Start - s2.Start) * s <= s * s))
                    {

                        var startStartEqual = s1.Start.Equals(s2.Start);
                        var startEndEqual = s1.Start.Equals(s2.End);
                        var endStartEqual = s1.End.Equals(s2.Start);
                        var endEndEqual = s1.End.Equals(s2.End);
                        if ( ((startStartEqual ? 1 :0 ) + (startEndEqual ? 1 : 0) + (endStartEqual ? 1 : 0) + (endEndEqual ? 1 : 0)) == 1 ) return false;

                        return true;
                    }
                        

                // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }
            

            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (rxs == 0 && !(qpxr == 0))
                return false;

            // t = (q - p) x s / (r x s)
            var t = (s2.Start - s1.Start).Cross(s) / rxs;

            // u = (q - p) x r / (r x s)

            var u = (s2.Start - s1.Start).Cross(r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if ((rxs != 0) && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                // We can calculate the intersection point using either t or u.
                intersection = s1.Start + t * r;

                if (intersection.Equals(s1.Start) || intersection.Equals(s1.End) || intersection.Equals(s2.Start) || intersection.Equals(s2.End)) return false;

                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }
    }
}
