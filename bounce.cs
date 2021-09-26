using System;
using System.Drawing;
using System.Windows.Forms;

namespace BouncyWindow
{
    public class Bounce : Control
    {
        private class Vector2D
        {
            public float X { get; set; }
            public float Y { get; set; }

            public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
            public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
            public static Vector2D operator *(Vector2D a, Vector2D b) => new Vector2D(a.X * b.X, a.Y * b.Y);
            public static Vector2D operator /(Vector2D a, Vector2D b) => new Vector2D(a.X / b.X, a.Y / b.Y);
            public static Vector2D operator ~(Vector2D a) => new Vector2D(-a.X, -a.Y);

            public void clip(Vector2D bounds)
            {
                clip(bounds, ~bounds);
            }
            public void clip(Vector2D upper, Vector2D lower)
            {
                if (X > upper.X) X = upper.X;
                if (X < lower.X) X = lower.X;
                if (Y > upper.Y) Y = upper.Y;
                if (Y < lower.Y) Y = lower.Y;
            }

            public Point ToPoint() => new Point((int)X, (int)Y);

            public Vector2D(float x, float y)
            {
                X = x;
                Y = y;
            }
            public Vector2D(Point p)
            {
                X = p.X;
                Y = p.Y;
            }
        }

        Vector2D Gravity { get; set; }     = new Vector2D(0, 1f);
        Vector2D Elasticity { get; set; }  = new Vector2D(0.5f, 0.6f);
        Vector2D MaxVelocity { get; set; } = new Vector2D(100f, 100f);
        Vector2D Friction { get; set; }    = new Vector2D(0.7f, 1f);

        Vector2D mouseDragStart = new Vector2D(0, 0);
        Timer updateTimer = new Timer();
        Form parent = null;
        Vector2D velocity = new Vector2D(0, 0);
        bool enabled = true;

        public Bounce()
        {
            ParentChanged += parentChanged;
        }

        private void parentChanged(object sender, EventArgs e)
        {
            if (parent != null)
                throw new Exception("Parent form cannot be changed!");

            parent = Parent as Form ?? throw new Exception("Control must be added to a form!");

            parent.ResizeBegin += dragStart;
            parent.ResizeEnd   += dragEnd;

            updateTimer.Tick += update;
            updateTimer.Interval = 10;
            updateTimer.Start();
        }

        private void dragStart(object sender, EventArgs e)
        {
            mouseDragStart = new Vector2D(Cursor.Position);

            // Prevent location updates when user drags form
            enabled = false;
        }
        private void dragEnd(object sender, EventArgs e)
        {
            velocity = new Vector2D(Cursor.Position) - mouseDragStart;
            enabled  = true;
        }

        private void update(object sender, EventArgs e)
        {
            if (Enabled)
            {
                if (enabled)
                {
                    velocity += Gravity;
                    velocity.clip(MaxVelocity);

                    var screenBounds = Screen.GetWorkingArea(parent.Location);

                    Vector2D nextLoc = new Vector2D(parent.Location) + velocity;

                    if (nextLoc.X <= screenBounds.Left || nextLoc.X >= screenBounds.Right - parent.Size.Width)
                        velocity.X *= -Elasticity.X;

                    if (nextLoc.Y >= screenBounds.Bottom - parent.Size.Height || nextLoc.Y <= screenBounds.Top)
                    {
                        velocity.Y *= -Elasticity.Y;
                        velocity *= Friction;
                    }

                    nextLoc.clip(new Vector2D(screenBounds.Right - parent.Size.Width, screenBounds.Bottom - parent.Size.Height), new Vector2D(screenBounds.Left, screenBounds.Top));

                    parent.Location = nextLoc.ToPoint();
                }
                else
                {
                    mouseDragStart = new Vector2D(Cursor.Position);
                }
            }
        }
    }
}