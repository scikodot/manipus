﻿using Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Graphics
{
    public class AxesWidget
    {
        public Axis axisX, axisY, axisZ;
        public IRenderable Parent;

        public AxesWidget()
        {
            axisX = new Axis(Vector4.UnitW, new Vector4(0.3f, 0, 0, 1), new Vector4(1, 0, 0, 1));
            axisY = new Axis(Vector4.UnitW, new Vector4(0, 0.3f, 0, 1), new Vector4(0, 1, 0, 1));
            axisZ = new Axis(Vector4.UnitW, new Vector4(0, 0, 0.3f, 1), new Vector4(0, 0, 1, 1));
        }

        public void Attach(IRenderable renderable)
        {
            Parent = renderable;
        }

        public void Transform(Matrix4 view, Matrix4 proj, Vector2 posCurr, MouseState stateCurr)
        {
            var parentState = Parent.State;
            axisX.Transform(ref parentState, view, proj, posCurr, stateCurr);  // TODO: prioritize axes polling, so that those with smaller depth go first
            //axisY.Transform(ref parentState, view, proj, posCurr, stateCurr);
            //axisZ.Transform(ref parentState, view, proj, posCurr, stateCurr);
            Parent.State = parentState;
        }
    }

    public class Axis
    {
        public Vector4 Start, End;
        public Vector2 StartNDC, EndNDC;
        public PlainModel Model;
        public float CursorDist;

        private float _scale = 1;
        public float Scale
        {
            get => _scale;
            set
            {
                // determine change in scale
                var ratio = value / _scale;  // TODO: check for zero, i.e. when camera is aligned with the widget

                // apply that change
                End = ratio * (End - Start) + Start;

                // and store the new scale
                _scale = value;
            }
        }

        public bool FirstClick = true, Started;
        public Vector3 Offset;

        public Axis(Vector4 start, Vector4 end, Vector4 color)
        {
            Start = start;
            End = end;
            Model = new PlainModel(Window.lineShader, new float[]
            {
                start.X, start.Y, start.Z,   color.X, color.Y, color.Z, color.W,
                end.X, end.Y, end.Z,    color.X, color.Y, color.Z, color.W
            });
        }

        public bool Poll(Matrix4 view, Matrix4 proj, Vector2 cursorPos)  // TODO: optimize, refactor
                                                                         // TODO: (optionally) check distance between ray and axis; if small, the axis is active
        {
            // transform start point to NDC
            var startView = Start * view;
            var startProj = startView * proj;
            startProj /= startProj.W;
            StartNDC = new Vector2(startProj.X, startProj.Y);  // TODO: why no minus at Y here?

            // transform end point to NDC
            var endView = End * view;
            var endProj = endView * proj;
            endProj /= endProj.W;
            EndNDC = new Vector2(endProj.X, endProj.Y);

            CursorDist = Vector2.Distance(cursorPos, EndNDC);
            return CursorDist < 0.1f;
        }

        public void Transform(ref Matrix4 state, Matrix4 view, Matrix4 proj, Vector2 posCurr, MouseState stateCurr)  // TODO: optimize, remove state
        {
            var axisActive = Poll(view, proj, posCurr);
            //Console.SetCursorPosition(0, 10);
            //Console.WriteLine($"Axis X is {axisActive}");
            //Console.WriteLine($"Mouse button is {stateCurr.RightButton}");

            if ((axisActive || Started) && stateCurr.RightButton == ButtonState.Pressed)
            {
                Started = true;

                // find the new mouse point projection onto the X axis (in NDC space)
                var mNew = posCurr;
                var a = EndNDC - StartNDC;
                var n = mNew - EndNDC;
                var v = a.Normalized() * Vector2.Dot(a, n);

                // cast a ray from the near plane to the far plane
                var raycastRes = Raycast(EndNDC + v, view, proj);

                // find the point of intersection of the ray and the XY plane (in World space)
                var p0 = Vector3.Zero;  // a point on the XY plane
                var l0 = raycastRes.Item1.Xyz;  // a point on the near plane
                var norm = Vector3.UnitZ;  // the normal of the XY plane
                var l = raycastRes.Item2.Xyz.Normalized();  // a direction of the ray

                var nom = Vector3.Dot(p0 - l0, norm);
                var denom = Vector3.Dot(l, norm);

                Vector3 intersection = default;
                if (denom != 0)
                {
                    // a unique point of intersection
                    var d = nom / denom;
                    intersection = l0 + l * d;
                }
                else
                {
                    if (nom == 0)
                    {
                        // the ray is on the plane, i.e. infinite amount of intersections

                    }
                    else
                    {
                        // the ray is parallel to the plane, i.e. no intersections

                    }
                }

                //Vector3 intersection = LinesIntersection(raycastRes.Item1.Xyz, raycastRes.Item2.Xyz, Vector3.Zero, Vector3.UnitX);
                //Console.SetCursorPosition(0, 10);
                //Console.WriteLine("({0:0.000}, {1:0.000}, {2:0.000})", intersection.X, intersection.Y, intersection.Z);

                if (FirstClick)
                {
                    Offset = new Vector3(intersection.X, 0, 0) - End.Xyz;
                    FirstClick = false;
                }
                else
                {
                    // project the intersecton point onto the X axis (in World space)
                    Vector3 translation = new Vector3(intersection.X, 0, 0) - Offset - End.Xyz;

                    Start += new Vector4(translation);
                    End += new Vector4(translation);

                    state.M14 += translation.X;
                    Model.State = new Matrix4(
                        new Vector4(Scale, 0, 0, state.M14),
                        new Vector4(0, Scale, 0, 0),
                        new Vector4(0, 0, Scale, 0),
                        new Vector4(0, 0, 0, 1));
                }
            }
            else
            {
                Started = false;
                FirstClick = true;

                Model.State = new Matrix4(
                        new Vector4(Scale, 0, 0, Model.State.M14),
                        new Vector4(0, Scale, 0, 0),
                        new Vector4(0, 0, Scale, 0),
                        new Vector4(0, 0, 0, 1));
            }
        }

        public Vector3 LinesIntersection(Vector3 p1, Vector3 a1, Vector3 p2, Vector3 a2)
        {
            var f1 = Vector3.Dot(a1, p1 - p2);
            var f2 = Vector3.Dot(a1, a1);
            var f3 = Vector3.Dot(a1, a2);
            var f4 = Vector3.Dot(a2, p1 - p2);
            var f5 = Vector3.Dot(a2, a2);

            var denom = f3 * f3 - f2 * f5;
            if (denom != 0)
            {
                //var t1 = (f1 * f5 - f3 * f4) / denom;
                var t2 = (f1 * f3 - f2 * f4) / denom;

                return p2 + t2 * a2;
            }
            else
            {
                // ???
                return default;
            }
        }

        public (Vector4, Vector4) Raycast(Vector2 ndc, Matrix4 view, Matrix4 proj)  // TODO: optimize
        {
            var vStart = new Vector4(ndc.X, ndc.Y, -1, 1);
            var vEnd = new Vector4(ndc.X, ndc.Y, 0, 1);

            var projInv = Matrix4.Invert(Matrix4.Transpose(proj));
            var viewInv = Matrix4.Invert(Matrix4.Transpose(view));

            var rayStartCamera = projInv * vStart;
            rayStartCamera /= rayStartCamera.W;
            var rayStartWorld = viewInv * rayStartCamera;
            rayStartWorld /= rayStartWorld.W;

            var rayEndCamera = projInv * vEnd;
            rayEndCamera /= rayEndCamera.W;
            var rayEndWorld = viewInv * rayEndCamera;
            rayEndWorld /= rayEndWorld.W;

            var rayDir = Vector4.Normalize(rayEndWorld - rayStartWorld);

            return (rayStartWorld, rayDir);
        }
    }
}