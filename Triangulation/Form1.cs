﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Triangulation {
    public partial class Form1 : Form {
        Point CurrentPoint; //Current Position
        Point LastPoint;
        Graphics graphic;
        List<Point> polynom_points;

        struct Triangle {
            public Point point1;
            public Point point2;
            public Point point3;
            public HashSet<Point> set_of_points;

            public Triangle (Point point1, Point point2, Point point3) {
                this.point1 = point1;
                this.point2 = point2;
                this.point3 = point3;
                set_of_points = new HashSet<Point> ();
                set_of_points.Add (point1);
                set_of_points.Add (point2);
                set_of_points.Add (point3);
            }
        }

        List<Triangle> list_of_triangles;
        public Form1 () {
            InitializeComponent ();
            area.Image = new Bitmap (area.Width, area.Height);
            graphic = Graphics.FromImage (area.Image);
            graphic.Clear (Color.White);

            polynom_points = new List<Point> ();
            list_of_triangles = new List<Triangle> ();
        }

        private void Button1_Click (object sender, EventArgs e) {
            graphic.Clear (Color.White);
            area.Invalidate ();
            polynom_points.Clear ();
            list_of_triangles.Clear ();
        }

        private void area_MouseDown (object sender, MouseEventArgs e) {
            CurrentPoint = e.Location;
            polynom_points.Add (CurrentPoint);
            graphic.FillEllipse (new SolidBrush (Color.Black), new Rectangle (CurrentPoint.X, CurrentPoint.Y, 3, 3));
            area.Invalidate ();
        }

        private static int MyCompare (Point point1, Point point2) {
            if (point1.X < point2.X)
                return -1;
            else if (point1.X > point2.X)
                return 1;
            else
                return 0;
        }

        private bool isNeighbor (Point point1, Point point2) {
            int i1 = 0;
            int i2 = 0;
            for (int i = 0; i < polynom_points.Count; i++) {
                if (polynom_points[i] == point1)
                    i1 = i;
                else if (polynom_points[i] == point2)
                    i2 = i;
            }

            if (Math.Abs (i2 - i1) == 1 || Math.Abs (i2 - i1) == polynom_points.Count - 1)
                return true;

            for (int i = 0; i < list_of_triangles.Count; i++) {
                if (list_of_triangles[i].set_of_points.Contains (point1) && list_of_triangles[i].set_of_points.Contains (point2))
                    return true;
            }

            return false;
        }

        private bool RightCheck (Point O, Point A, Point B) {
            int ox = O.X;
            int oy = O.Y;
            int xa = A.X;
            int ya = A.Y;
            int xb = B.X;
            int yb = B.Y;

            int xam = xa - ox;
            int yam = ya - oy;
            int xbm = xb - ox;
            int ybm = yb - oy;

            if ((ybm * xam - xbm * yam) >= 0) {
                return true;
            } else {
                return false;
            }
        }

        private bool Check_Cross (Point A, Point B, Point C, Point D) {

            bool f1 = false, f2 = false;
            if (RightCheck (A, B, C) && !(RightCheck (A, B, D))) {
                f1 = true;
            }
            if (!(RightCheck (A, B, C)) && (RightCheck (A, B, D))) {
                f1 = true;
            }

            if (RightCheck (C, D, A) && !(RightCheck (C, D, B))) {
                f2 = true;
            }
            if (!(RightCheck (C, D, A)) && (RightCheck (C, D, B))) {
                f2 = true;
            }

            if (f1 == true && f2 == true) {
                return true;
            } else {
                return false;
            }

        }

        private bool isInPoly (Point p) {

            int count_cross = 0;
            Point P = p;
            Point start = this.polynom_points.First ();
            Point old = this.polynom_points.First ();
            Point D = new Point (area.Width - 1, P.Y);
            for (int i = 1; i < polynom_points.Count (); i++) {

                if (this.Check_Cross (old, this.polynom_points.ElementAt (i), P, D) == true) {
                    count_cross += 1;
                }
                old = this.polynom_points.ElementAt (i);
            }

            if (this.Check_Cross (old, start, P, D) == true) {
                count_cross += 1;
            }
            if (count_cross % 2 == 1) {
                return true;
            } else {
                return false;
            }
        }

        public bool isAngleInside (Point point1, Point point2, Point point3, Point center) {
            int D = (point3.X - point1.X) * (point2.Y - point1.Y) - (point3.Y - point1.Y) * (point2.X - point1.X);
            if (point1.Y > center.Y)
                return D < 0;
            else
                return D > 0;
        }

        Point mass_center () {
            int size = polynom_points.Count;
            int x = 0, y = 0;
            for (int i = 0; i < size; i++) {
                x += polynom_points[i].X;
                y += polynom_points[i].Y;
            }
            return new Point (x / size, y / size);
        }

        private void Button2_Click (object sender, EventArgs e) {
            List<Point> sortPoints = new List<Point> (polynom_points);
            sortPoints.Sort (MyCompare);
            Stack<Point> s = new Stack<Point> ();
            Point fstPoint = sortPoints[0];
            Point prevTPoint = new Point ();
            s.Push (fstPoint);
            int i = 1;
            while (true) {
                if (s.Count == 1) {
                    prevTPoint = s.Peek ();
                    s.Push (sortPoints[i]);
                    i++;
                    continue;
                }

                if (isNeighbor (sortPoints[i], s.Peek ()) && !isNeighbor (sortPoints[i], fstPoint)) {
                    if (!isAngleInside (sortPoints[i], s.Peek (), prevTPoint, fstPoint)) {
                        prevTPoint = s.Peek ();
                        s.Push (sortPoints[i]);
                        i++;
                        continue;
                    }
                    list_of_triangles.Add (new Triangle (sortPoints[i], s.Peek (), prevTPoint));
                    s.Pop ();
                    if (s.Count > 1)
                        prevTPoint = s.ElementAt (1);
                    else
                        prevTPoint = fstPoint;
                    continue;
                }

                if (isNeighbor (sortPoints[i], fstPoint) && !isNeighbor (sortPoints[i], s.Peek ())) {
                    Point lstPoint = s.Peek ();
                    while (true) {
                        list_of_triangles.Add (new Triangle (sortPoints[i], s.Peek (), prevTPoint));
                        if (prevTPoint == fstPoint) {
                            while (s.Count > 0)
                                s.Pop ();
                            break;
                        }
                        s.Pop ();
                        prevTPoint = s.ElementAt (1);
                    }
                    s.Push (lstPoint);
                    s.Push (sortPoints[i]);
                    fstPoint = lstPoint;
                    prevTPoint = fstPoint;
                    i++;
                    continue;
                }

                if (isNeighbor (sortPoints[i], fstPoint) && isNeighbor (sortPoints[i], s.Peek ())) {
                    while (true) {
                        list_of_triangles.Add (new Triangle (sortPoints[i], s.Peek (), prevTPoint));
                        s.Pop ();
                        if (s.Count > 1)
                            prevTPoint = s.ElementAt (1);
                        else
                            break;
                    }
                    break;
                }

                s.Push (sortPoints[i]);
                i++;

            }

            for (int j = 0; j < list_of_triangles.Count; j++) {
                Pen p = new Pen (Color.Black);
                graphic.DrawLine (p, list_of_triangles[j].point1, list_of_triangles[j].point2);
                graphic.DrawLine (p, list_of_triangles[j].point1, list_of_triangles[j].point3);
                graphic.DrawLine (p, list_of_triangles[j].point2, list_of_triangles[j].point3);
            }

            area.Invalidate ();

        }

    }
}
