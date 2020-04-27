﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Graphics;
using Logic.PathPlanning;

namespace Logic
{
    public struct ManipData
    {
        public int N;
        public LinkData[] Links;
        public JointData[] Joints;
        public Vector3[] JointAxes;
        public Vector3[] JointPositions;
        public Vector3 Goal;
        public bool ShowTree;
    }

    public struct LinkData
    {
        public ComplexModel Model;
        public float Length;
    }

    public struct JointData
    {
        public ComplexModel Model;
        public float Length;
        public float q;
        public Vector2 qRanges;
    }

    public struct ObstData
    {
        public float Radius;
        public Vector3 Center;
        public int PointsNum;
        public bool ShowCollider;
    }

    public struct AlgData
    {
        public int InverseKinematicsSolverID;
        public float StepSize;
        public float Precision;
        public int MaxTime;

        public int PathPlannerID;
        public int AttrNum;
        public int k;
        public float d;
    }

    public enum JointType
    {
        Prismatic,  // Translation
        Revolute,  // Rotation
        Cylindrical,  // Translation & rotation
        Spherical,  // Allows three degrees of rotational freedom about the center of the joint. Also known as a ball-and-socket joint
        Planar  // Allows relative translation on a plane and relative rotation about an axis perpendicular to the plane
    }

    public class Link
    {
        public ComplexModel Model;
        public float Length;  // TODO: must be names Size or something like that; Length is not suitable

        public ImpDualQuat State;

        public Link(LinkData data)
        {
            Model = data.Model;
            Length = data.Length;
        }

        public Link ShallowCopy()
        {
            return (Link)MemberwiseClone();
        }
    }

    public class Joint
    {
        public ComplexModel Model;
        public float Length;

        public float q;
        public float[] qRanges;  // TODO: consider switching to Vector2 instead of array; array has a bit of overhead

        public ImpDualQuat State;

        public Vector3 Position { get; set; }
        public Vector3 Axis { get; set; }

        public Joint(JointData data)
        {
            Model = data.Model;
            Length = data.Length;
            q = data.q;
            qRanges = new float[2] { data.qRanges.X, data.qRanges.Y };
        }

        public Joint ShallowCopy()
        {
            return (Joint)MemberwiseClone();
        }
    }

    public class Manipulator
    {
        public Vector3 Base;
        public Link[] Links;
        public Joint[] Joints;
        public float WorkspaceRadius;

        public Vector3[] InitialAxes;
        public Vector3[] InitialPositions;

        public Vector3 Goal;
        public List<Vector3> Path;
        public List<Vector> Configs;
        public Tree Tree;
        public List<Attractor> Attractors;

        public MotionController Controller { get; set; }

        public int posCounter = 0;

        public Manipulator(ManipData data)
        {
            Base = data.JointPositions[0];
            Links = Array.ConvertAll(data.Links, x => new Link(x));
            Joints = Array.ConvertAll(data.Joints, x => new Joint(x));

            InitialAxes = data.JointAxes;
            InitialPositions = data.JointPositions;

            for (int i = 0; i < Joints.Length; i++)
            {
                Joints[i].Axis = data.JointAxes[i];
                Joints[i].Position = data.JointPositions[i];
            }

            _q = new Vector(Joints.Select(x => x.q).ToArray());

            UpdateState();

            WorkspaceRadius = Links.Sum(link => link.Length) + Joints.Sum(joint => joint.Length);
            
            Goal = data.Goal;
        }

        public void UpdateState()
        {
            DKP = new Vector3[Joints.Length];

            ImpDualQuat quat;
            ImpDualQuat init = new ImpDualQuat(Base);

            // joints
            for (int i = 0; i < Joints.Length; i++)
            {
                quat = init;

                for (int j = 0; j < i; j++)
                {
                    quat *= new ImpDualQuat(InitialPositions[j + 1] - InitialPositions[j]);  // TODO: this *may* cause inappropriate behaviour; consider initPos[j + 1] - initPos[i]
                    quat *= new ImpDualQuat(quat.Conjugate, InitialAxes[j], quat.Translation, Joints[j].Position, -Joints[j].q);  // TODO: optimize; probably, conjugation can be avoided
                }

                quat *= new ImpDualQuat(InitialAxes[i], -Joints[i].q);

                quat *= ImpDualQuat.Align(Joints[0].Axis, InitialAxes[i]);

                Joints[i].State = quat;
                Joints[i].Axis = quat.Rotate(Joints[0].Axis);
                Joints[i].Position = DKP[i] = quat.Translation;
            }

            // links
            for (int i = 0; i < Links.Length; i++)
            {
                quat = init;

                for (int j = 0; j <= i; j++)
                {
                    quat *= j == 0 ?
                        new ImpDualQuat(Joints[0].Length / 2 * Vector3.UnitY) :
                        new ImpDualQuat(InitialPositions[j] - InitialPositions[j - 1]);
                    quat *= new ImpDualQuat(quat.Conjugate, InitialAxes[j], quat.Translation, Joints[j].Position, -Joints[j].q);
                }

                Links[i].State = quat;
            }

            // gripper
            GripperPos = DKP[Joints.Length - 1];
        }

        public Vector3 GripperPos { get; private set; }

        public Vector3[] DKP { get; private set; }

        private Vector _q;
        public Vector q
        {
            get => _q;
            set
            {
                _q = value;
                for (int i = 0; i < Joints.Length; i++)
                    Joints[i].q = value[i];

                UpdateState();
            }
        }

        public bool InWorkspace(Vector3 point)  // TODO: not used anywhere; fix
        {
            return point.DistanceTo(Vector3.Zero) - point.DistanceTo(Vector3.Zero) <= WorkspaceRadius;
        }

        public float DistanceTo(Vector3 p)
        {
            return GripperPos.DistanceTo(p);
        }

        public void Render(Shader shader)
        {
            if (Configs != null)
                q = Configs[posCounter < Configs.Count - 1 ? posCounter++ : posCounter];

            shader.Use();

            OpenTK.Matrix4 model;

            // joints
            for (int i = 0; i < Joints.Length; i++)
            {
                model = Joints[i].State.ToMatrix(true);
                shader.SetMatrix4("model", ref model);
                Joints[i].Model.Render(shader, MeshMode.Solid | MeshMode.Wireframe);
            }

            // links
            for (int i = 0; i < Links.Length; i++)
            {
                model = Links[i].State.ToMatrix(true);
                shader.SetMatrix4("model", ref model);
                Links[i].Model.Render(shader, MeshMode.Solid | MeshMode.Wireframe);
            }
        }

        public Manipulator DeepCopy()
        {
            Manipulator manip = (Manipulator)MemberwiseClone();

            manip.Links = Array.ConvertAll(Links, x => x.ShallowCopy());
            manip.Joints = Array.ConvertAll(Joints, x => x.ShallowCopy());

            if (Path != null)
                manip.Path = new List<Vector3>(Path);

            return manip;
        }
    }
}
