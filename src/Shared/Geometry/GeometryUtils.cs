#if !CLOUD_BUILD
using Autodesk.AutoCAD.Geometry;
#endif

namespace Shared.Geometry
{
#if !CLOUD_BUILD
    /// <summary>
    /// 几何工具类
    /// </summary>
    public static class GeometryUtils
#else
    /// <summary>
    /// 几何工具类（云编译存根）
    /// </summary>
    public static class GeometryUtils
#endif
    {
#if !CLOUD_BUILD
        /// <summary>
        /// 计算两点距离
        /// </summary>
        public static double Distance(Point3d p1, Point3d p2)
        {
            return p1.GetDistanceTo(p2);
        }

        /// <summary>
        /// 计算中点
        /// </summary>
        public static Point3d MidPoint(Point3d p1, Point3d p2)
        {
            return new Point3d(
                (p1.X + p2.X) / 2,
                (p1.Y + p2.Y) / 2,
                (p1.Z + p2.Z) / 2);
        }

        /// <summary>
        /// 向量点积
        /// </summary>
        public static double DotProduct(Vector3d v1, Vector3d v2)
        {
            return v1.Dot(v2);
        }

        /// <summary>
        /// 向量叉积
        /// </summary>
        public static Vector3d CrossProduct(Vector3d v1, Vector3d v2)
        {
            return v1.Cross(v2);
        }
#endif

        /// <summary>
        /// 判断点是否在矩形内
        /// </summary>
        public static bool IsPointInRect(Point2d point, Rect2d rect)
        {
            return point.X >= rect.Min.X && point.X <= rect.Max.X &&
                   point.Y >= rect.Min.Y && point.Y <= rect.Max.Y;
        }

        /// <summary>
        /// 计算矩形包围盒
        /// </summary>
        public static Rect2d GetBoundingBox(IEnumerable<Point2d> points)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            foreach (var p in points)
            {
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
            }

            return new Rect2d(new Point2d(minX, minY), new Point2d(maxX, maxY));
        }

        /// <summary>
        /// 旋转向量
        /// </summary>
        public static Vector2d Rotate(Vector2d vector, double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            return new Vector2d(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos);
        }

        /// <summary>
        /// 判断两线段是否相交
        /// </summary>
        public static bool SegmentsIntersect(Point2d a1, Point2d a2, Point2d b1, Point2d b2)
        {
            var d1 = Direction(b1, b2, a1);
            var d2 = Direction(b1, b2, a2);
            var d3 = Direction(a1, a2, b1);
            var d4 = Direction(a1, a2, b2);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            return false;
        }

        private static double Direction(Point2d p1, Point2d p2, Point2d p3)
        {
            return (p3.X - p1.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p3.Y - p1.Y);
        }
    }

    /// <summary>
    /// 2D 矩形
    /// </summary>
    public struct Rect2d
    {
        public Point2d Min { get; }
        public Point2d Max { get; }

        public Rect2d(Point2d min, Point2d max)
        {
            Min = min;
            Max = max;
        }

        public double Width => Max.X - Min.X;
        public double Height => Max.Y - Min.Y;
    }
}
