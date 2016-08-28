using System.Collections.Generic;

namespace CityGen.QuadTree
{
    interface IBoundingBox
    {
        Rectangle BoundingBox { get; }
    }

    class QuadTree<T> where T : IBoundingBox
    {
        private QuadTree<T>[] _nodes;
        private Rectangle _rectangle;
        private List<T> _objects;
        private int _level;

        public QuadTree(Rectangle area) : this(area, 0)
        { }

        private QuadTree(Rectangle area, int level)
        {
            _objects = new List<T>();
            _nodes = null;
            _rectangle = area;
            _level = level;
        }

        public void Insert(T obj)
        {
            if (_nodes != null)
            {
                int index = GetIndex(obj);
                if (index != -1)
                {
                    _nodes[index].Insert(obj);
                    return;
                }
            }

            _objects.Add(obj);

            if (_objects.Count > 10 && _level < 4)
            {
                if (_nodes == null)
                    Split();

                int i = 0;
                while (i < _objects.Count)
                {
                    int index = GetIndex(_objects[i]);
                    if (index != -1)
                    {
                        var objToMove = _objects[i];
                        _objects.Remove(objToMove);
                        _nodes[index].Insert(objToMove);
                    }
                    else
                        i++;
                }
            }
        }
        
        public IReadOnlyList<T> GetObjects(T obj)
        {
            var all = new List<T>(_objects);
            all.AddRange(GetChildObjects(obj));
            all.Remove(obj);
            return all;
        }

        private IReadOnlyList<T> GetChildObjects(T obj)
        {
            if (_nodes == null)
                return new T[0];

            int index = GetIndex(obj);
            if (index != -1)
            {
                return _nodes[index].GetObjects(obj);
            }
            else
            {
                var all = new List<T>();
                for (int i = 0; i < _nodes.Length; i++)
                    all.AddRange(_nodes[i].GetObjects());

                return all;
            }
        }

        public IReadOnlyList<T> GetObjects()
        {
            var all = new List<T>(_objects);
            if (_nodes != null)
                for (int i = 0; i < _nodes.Length; i++)
                    all.AddRange(_nodes[i].GetObjects());

            return all;
        }

        private int GetIndex(T obj)
        {
            int index = -1;
            int vMid = _rectangle.LeftTop.X + (_rectangle.Width / 2);
            int hMid = _rectangle.LeftTop.Y + (_rectangle.Height / 2);

            var box = obj.BoundingBox;

            bool topQuadrant = box.LeftTop.Y > hMid;
            bool bottomQuadrant = box.LeftTop.Y < hMid && box.LeftTop.Y + box.Height < hMid;

            if (box.LeftTop.X < vMid && box.LeftTop.X + box.Width < vMid)
            {
                if (topQuadrant)
                    index = 0;
                else if (bottomQuadrant)
                    index = 2;
            }
            else if (box.LeftTop.X > vMid)
            {
                if (topQuadrant)
                    index = 1;
                else if (bottomQuadrant)
                    index = 3;
            }

            return index;
        }

        private void Split()
        {
            int subWidth = (int)(_rectangle.Width / 2);
            int subHeight = (int)(_rectangle.Height / 2);
            int x = _rectangle.LeftTop.X;
            int y = _rectangle.LeftTop.Y;

            // 0 | 1
            // -----
            // 2 | 3
            _nodes = new QuadTree<T>[4] {
                new QuadTree<T>(new Rectangle(new Vector(x , y), subWidth, subHeight), _level + 1),
                new QuadTree<T>(new Rectangle(new Vector(x + subWidth, y), subWidth, subHeight), _level + 1),
                new QuadTree<T>(new Rectangle(new Vector(x, y + subHeight), subWidth, subHeight), _level + 1),
                new QuadTree<T>(new Rectangle(new Vector(x + subWidth, y + subHeight), subWidth, subHeight), _level + 1),
            };
        }
    }
}
