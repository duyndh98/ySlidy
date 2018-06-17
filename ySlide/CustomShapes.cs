using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace ySlidy
{
    class Triangle : Shape
    {
        private Point start;
        public Point Start { get; set; }
        protected override Geometry DefiningGeometry
        {
            get { return GenerateTriangleGeometry(); }
        }

        private Geometry GenerateTriangleGeometry()
        {
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = start;
            pathFigure.IsClosed = true;

            LineSegment ls1 = new LineSegment();
            ls1.Point = new Point(start.X, start.Y + this.Height);
            pathFigure.Segments.Add(ls1);
            LineSegment ls2 = new LineSegment();
            ls2.Point = new Point(start.X + this.Width, start.Y + this.Height);
            pathFigure.Segments.Add(ls2);
            pathGeometry.Figures.Add(pathFigure);

            //LineSegment ls1 = new LineSegment();
            //ls1.Point = new Point(100, 100);
            //pathFigure.Segments.Add(ls1);
            //LineSegment ls2 = new LineSegment();
            //ls2.Point = new Point(100, 50);
            //pathFigure.Segments.Add(ls2);
            //pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }
    }

    class Arrow : Shape
    {
        private Point start;
        public Point Start { get; set; }
        protected override Geometry DefiningGeometry
        {
            get { return GenerateTriangleGeometry(); }
        }

        private Geometry GenerateTriangleGeometry()
        {
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(start.X, start.Y + this.Height/4);
            pathFigure.IsClosed = true;
            

            LineSegment ls1 = new LineSegment();
            ls1.Point = new Point(start.X, start.Y + this.Height *3/4);
            LineSegment ls2 = new LineSegment();
            ls2.Point = new Point(ls1.Point.X + this.Width / 2, ls1.Point.Y);
            LineSegment ls3 = new LineSegment();
            ls3.Point = new Point(ls2.Point.X, ls2.Point.Y + this.Height / 4);
            LineSegment ls4 = new LineSegment();
            ls4.Point = new Point(ls3.Point.X + this.Width / 2, ls3.Point.Y - this.Height / 2);
            LineSegment ls5 = new LineSegment();
            ls5.Point = new Point(ls4.Point.X - this.Width / 2, ls4.Point.Y - this.Height/2);
            LineSegment ls6 = new LineSegment();
            ls6.Point = new Point(ls5.Point.X, ls5.Point.Y + this.Height / 4);

            pathFigure.Segments.Add(ls1);
            pathFigure.Segments.Add(ls2);
            pathFigure.Segments.Add(ls3);
            pathFigure.Segments.Add(ls4);
            pathFigure.Segments.Add(ls5);
            pathFigure.Segments.Add(ls6);

            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }
    }

    class Heart : Shape
    {
        
        private Point start;
        public Point Start { get; set; }
        protected override Geometry DefiningGeometry
        {
            get { return GenerateTriangleGeometry(); }
        }

        private Geometry GenerateTriangleGeometry()
        {
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure1 = new PathFigure();
            pathFigure1.StartPoint = new Point(start.X + this.Width/2, start.Y + this.Height / 3);
            pathFigure1.IsClosed = false ;

            BezierSegment bs1 = new BezierSegment();
            bs1.Point1 = new Point(start.X + this.Width * 3 / 4, start.Y);
            bs1.Point2 = new Point(start.X + this.Width, start.Y + this.Height / 3);
            bs1.Point3 = new Point(start.X + this.Width / 2, start.Y + this.Height);
            pathFigure1.Segments.Add(bs1);

            PathFigure pathFigure2 = new PathFigure();
            pathFigure2.StartPoint = new Point(start.X + this.Width / 2, start.Y + this.Height / 3);
            pathFigure1.IsClosed = false;

            BezierSegment bs2 = new BezierSegment();
            bs2.Point1 = new Point(start.X + this.Width * 1 / 4, start.Y);
            bs2.Point2 = new Point(start.X, start.Y + this.Height / 3);
            bs2.Point3 = new Point(start.X + this.Width / 2, start.Y + this.Height);
            pathFigure2.Segments.Add(bs2);

            pathGeometry.Figures.Add(pathFigure1);
            pathGeometry.Figures.Add(pathFigure2);
            return pathGeometry;
        }
    }
}
