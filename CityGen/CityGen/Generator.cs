using CityGen.QuadTree;
using Priority_Queue;
using System;
using System.Collections.Generic;

namespace CityGen
{
    public class Generator
    {

        public int Height { get; private set; }
        public int Width { get; private set; }
        public int Seed { get; private set; }

        public float MaxAngle { get; set; }
        public float MinAngle { get; set; }
        public float AngleSteps { get; set; }

        public int MinNewSegments { get; set; }
        public int MaxNewSegments { get; set; }

        public int MinSegmentLength { get; set; }
        public int SegmentStepLength { get; set; }
        public int MaxSegmentLength { get; set; }

        public IReadOnlyList<Segment> Segments => _segments;

        private List<Segment> _segments;
        private QuadTree<Segment> _segmentsTree;
        private Random _random;
        private SimplePriorityQueue<Segment> _workQueue;

        private int _maxY = int.MinValue;
        private int _maxX = int.MinValue;
        private int _minX = int.MaxValue;
        private int _minY = int.MaxValue;

        public Generator(int seed, int width, int height, IEnumerable<Segment> startSegments)
        {
            Seed = seed;
            _random = new Random(seed);
            _segments = new List<Segment>();
            _segmentsTree = new QuadTree<Segment>(new Rectangle(new Vector(-width/2, height/2), width, height));

            Width = width;
            Height = height;

            _workQueue = new SimplePriorityQueue<Segment>();

            foreach(var s in startSegments)
            {
                _workQueue.Enqueue(s, s.Priority);
            }
        }

        public void DoStep()
        {
            var toDo = _workQueue.Count;

            for (var i = 0; i<toDo;++i)
            {
                var segment = _workQueue.Dequeue();

                var accepted = LocalConstraints(segment);
                if (!accepted) continue;

                _segments.Add(segment);
                _segmentsTree.Insert(segment);

                GenerateNewSegments(segment);
            }

            foreach(var s in _segments)
            {
                if(s.Start.X > _maxX)
                {
                    _maxX = s.Start.X;
                }
                if (s.End.X > _maxX)
                {
                    _maxX = s.Start.X;
                }

                if (s.Start.X < _minX)
                {
                    _minX = s.Start.X;
                }
                if (s.End.X < _minX)
                {
                    _minX = s.Start.X;
                }

                if (s.Start.Y > _maxY)
                {
                    _maxY = s.Start.X;
                }
                if (s.End.Y > _maxY)
                {
                    _maxY = s.Start.X;
                }

                if (s.Start.Y < _minY)
                {
                    _minY = s.Start.X;
                }
                if (s.End.Y < _minY)
                {
                    _minY = s.Start.X;
                }
            }
        }

        private bool LocalConstraints(Segment segment)
        {
            if(!( segment.End.X >= -Width/2 && segment.End.X <= Width/2 && segment.End.Y >= -Height / 2 && segment.End.Y <= Height / 2)) return false;

            if ((segment.End.X > _minX && segment.End.X < _maxX) || (segment.End.Y > _minY && segment.End.Y < _maxY)) return false;


            var segmentsToCheck = _segmentsTree.GetObjects(segment);

            foreach(var s in segmentsToCheck)
            {
                Vector intersection;
                if (Segment.Intersect(s, segment, out intersection))
                {
                    return false;
                }
            }

            return true;
        }

        private void GenerateNewSegments(Segment oldSegment)
        {
            var countOfPossibleAngles = (int)Math.Floor((MaxAngle - MinAngle) / AngleSteps);
            var possibleAngles = new List<float>();
            for(var i = 0; i <= countOfPossibleAngles; ++i)
            {
                possibleAngles.Add(MinAngle + AngleSteps * i);
            }

            var countOfPossibleLength = (int)Math.Floor((MaxSegmentLength - MinSegmentLength) / (float)SegmentStepLength);
            var possibleLengths = new List<int>();
            for (var i = 0; i <= countOfPossibleAngles; ++i)
            {
                possibleLengths.Add(MinSegmentLength + SegmentStepLength * i);
            }

            var amountOfGeneratedSegments = 0;
            while(amountOfGeneratedSegments < MaxNewSegments && possibleAngles.Count > 0)
            {
                if (_random.Next(0, 5) < 1) break; 

                var randomDirectionIndex = _random.Next(0, possibleAngles.Count);
                var direction = possibleAngles[randomDirectionIndex];

                var randomLengthIndex = _random.Next(0, possibleLengths.Count);

                var s = Segment.NewSegmentInDirection(oldSegment, direction, 0, possibleLengths[randomLengthIndex]);

                if ((s.End.X > _minX && s.End.X < _maxX) || (s.End.Y > _minY && s.End.Y < _maxY))
                {
                    s = Segment.NewSegmentInDirection(oldSegment, -direction, 0, possibleLengths[randomLengthIndex]);
                }
                _workQueue.Enqueue(s, s.Priority);

                possibleAngles.RemoveAt(randomDirectionIndex);
            }

            for(var i = amountOfGeneratedSegments; i < MinNewSegments && possibleAngles.Count > 0;++i )
            {
                var randomDirectionIndex = _random.Next(0, possibleAngles.Count);
                var direction = possibleAngles[randomDirectionIndex];

                var randomLengthIndex = _random.Next(0, possibleLengths.Count);

                var s = Segment.NewSegmentInDirection(oldSegment, direction, 0, possibleLengths[randomLengthIndex]);
                if ((s.End.X > _minX && s.End.X < _maxX) || (s.End.Y > _minY && s.End.Y < _maxY))
                {
                    s = Segment.NewSegmentInDirection(oldSegment, -direction, 0, possibleLengths[randomLengthIndex]);
                }
                _workQueue.Enqueue(s, s.Priority);

                possibleAngles.RemoveAt(randomDirectionIndex);
            }
        }
    }
}
