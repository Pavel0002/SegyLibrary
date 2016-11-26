using System;
using System.Collections.Generic;

namespace SegyLibrary
{
    public struct Point<T> where T : struct , IComparable, IComparable<T>, IEquatable<T>
    {
        public Point(T xVal, T yVal)
        {
            X = xVal;
            Y = yVal;
        }
        public T X { get; set; }

        public T Y { get; set; }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }
        public void ResetPoint()
        {
            X = default(T);
            Y = default(T);
        }
        public static bool operator ==(Point<T> point1, Point<T> point2)
        {
            return EqualityComparer<T>.Default.Equals(point1.X, point2.X) && EqualityComparer<T>.Default.Equals(point1.Y, point2.Y);
        }

        public static bool operator !=(Point<T> point1, Point<T> point2)
        {
            return !(point1 == point2);
        }
        // метод GetHashCode() применен к свойству не readonly... что неправильно?
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Point<T>))
                return false;
            var p = (Point<T>)obj;
            return EqualityComparer<T>.Default.Equals(X, p.X) && EqualityComparer<T>.Default.Equals(Y, p.Y);
        }
        
    }
}
