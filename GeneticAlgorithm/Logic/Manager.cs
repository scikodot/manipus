﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Logic
{
    class Manager
    {
        public static Manipulator[] Manipulators;
        public static Obstacle[] Obstacles;

        public static ManipData[] MD =
        {
            new ManipData
            {
                N = 4,
                Base = new System.Numerics.Vector3(-1.5f, 0, 1.5f),
                l = new float[10] { 0.2f, 2, 2, 2, 0, 0, 0, 0, 0, 0 },
                q = new float[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                q_ranges = new System.Numerics.Vector2[10]
                {
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180)
                },
                DH = new System.Numerics.Vector4[10]
                {
                    new System.Numerics.Vector4(0, 0.2f, 90, 0),
                    new System.Numerics.Vector4(-90, 0, 0, 2),
                    new System.Numerics.Vector4(0, 0, 0, 2),
                    new System.Numerics.Vector4(0, 0, 0, 2),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0)
                },
                Goal = new System.Numerics.Vector3(-2.2f, 0, -2),

                ShowTree = true
            },
            new ManipData
            {
                N = 4,
                Base = new System.Numerics.Vector3(2, 0, -2),
                l = new float[10] { 0.2f, 2, 2, 2, 0, 0, 0, 0, 0, 0 },
                q = new float[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                q_ranges = new System.Numerics.Vector2[10]
                {
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180),
                    new System.Numerics.Vector2(-180, 180)
                },
                DH = new System.Numerics.Vector4[10]
                {
                    new System.Numerics.Vector4(0, 0.2f, 90, 0),
                    new System.Numerics.Vector4(-90, 0, 0, 2),
                    new System.Numerics.Vector4(0, 0, 0, 2),
                    new System.Numerics.Vector4(0, 0, 0, 2),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0),
                    new System.Numerics.Vector4(0, 0, 0, 0)
                },
                Goal = new System.Numerics.Vector3(2.2f, 0, 2),

                ShowTree = true
            }
        };
        public static ObstData[] OD =
        {
            new ObstData
            {
                r = 1,
                c = new System.Numerics.Vector3(0, 1.5f, 0),
                points_num = 2000,

                ShowBounding = true
            },
            new ObstData
            {
                r = 1,
                c = new System.Numerics.Vector3(-2.2f, 2.5f, 0),
                points_num = 2000,

                ShowBounding = true
            },
            new ObstData
            {
                r = 1,
                c = new System.Numerics.Vector3(-2.2f, 0, -0.5f),
                points_num = 2000,

                ShowBounding = true
            }
        };

        public static AlgData AD = new AlgData
        {
            AttrNum = 10000,

            Precision = 0.02f,
            StepSize = 3,
            MaxTime = 300,

            k = 10000,
            d = 0.08f
        };

        public static void Initialize()
        {
            // manipulators
            Manipulators = new Manipulator[MD.Length];
            for (int i = 0; i < MD.Length; i++)
            {
                Manipulators[i] = new Manipulator(MD[i]);
            }
            /*Manipulators = new Manipulator[]
            {
                new Manipulator
                (
                    new Point(-1.5, 0, 1.5),
                    new double[] { 0.2, 2, 2, 2 },
                    Misc.ToRad(new double[] { 0, 0, 0, 0 }),
                    new double[,]
                    {
                        { -180, 180 },
                        { -180, 180 },
                        { -180, 180 },
                        { -180, 180 }
                    },
                    new TupleDH[]
                    {
                        new TupleDH((m) => { return m.q[0]; }, 0.2, Math.PI / 2, 0),
                        new TupleDH((m) => { return m.q[1] - Math.PI / 2; }, 0, 0, 2),
                        new TupleDH((m) => { return m.q[2]; }, 0, 0, 2),
                        new TupleDH((m) => { return m.q[3]; }, 0, 0, 2)
                    },
                    new Point(-2.2, 0, -2)
                ),
                new Manipulator
                (
                    new Point(2, 0, -2),
                    new double[] { 0.2, 2, 2, 2 },
                    Misc.ToRad(new double[] { 0, 0, 0, 0 }),
                    new double[,]
                    {
                        { -180, 180 },
                        { -180, 180 },
                        { -180, 180 },
                        { -180, 180 }
                    },
                    new TupleDH[]
                    {
                        new TupleDH((m) => { return m.q[0]; }, 0.2, Math.PI / 2, 0),
                        new TupleDH((m) => { return m.q[1] - Math.PI / 2; }, 0, 0, 2),
                        new TupleDH((m) => { return m.q[2]; }, 0, 0, 2),
                        new TupleDH((m) => { return m.q[3]; }, 0, 0, 2)
                    },
                    new Point(2.2, 0, 2)
                )
            };*/

            // obstacles
            Obstacles = new Obstacle[OD.Length];
            for (int i = 0; i < OD.Length; i++)
            {
                Obstacles[i] = new Obstacle(Primitives.Sphere(OD[i].r, new Point(OD[i].c.X, OD[i].c.Y, OD[i].c.Z), OD[i].points_num));
            }
            /*Obstacles = new Obstacle[]
            {
                new Obstacle(Primitives.Sphere(1, new Point(0, 1.5, 0), 2000)),
                new Obstacle(Primitives.Sphere(1, new Point(-2.2, 2.5, 0), 2000)),
                new Obstacle(Primitives.Sphere(1, new Point(-2.2, 0, -0.5), 2000))
            };*/
        }

        public static void Execute(Manipulator manip)
        {
            /*manip.States = new Dictionary<string, bool>
            {
                { "Goal", false },
                { "Attractors", false }
            };*/

            manip.Attractors = new List<Attractor>();

            Random rng = new Random();
            double work_radius = 2 * manip.l.Sum(), x, y_pos, y, z_pos, z;

            // adding main attractor
            Point AttrPoint = manip.Goal;

            double AttrWeight = manip.DistanceTo(manip.Goal);
            
            double r = 0.05 * Math.Pow(AttrWeight / manip.DistanceTo(manip.Goal), 4);
            Point[] AttrArea = Primitives.Sphere(r, AttrPoint, 64);

            manip.Attractors.Add(new Attractor(AttrPoint, AttrWeight, AttrArea, r));

            manip.States["Goal"] = true;

            // adding ancillary attractors
            while (manip.Attractors.Count < AD.AttrNum)
            {
                x = -work_radius + rng.NextDouble() * 2 * work_radius;
                y_pos = Math.Sqrt(work_radius * work_radius - x * x);
                y = -y_pos + rng.NextDouble() * 2 * y_pos;
                z_pos = Math.Sqrt(work_radius * work_radius - x * x - y * y);
                z = -z_pos + rng.NextDouble() * 2 * z_pos;
                
                //work_radius -= 2 * manip.l.Sum() / 20000;

                Point p = new Point(x, y, z) + manip.Base;
                bool collision = false;
                foreach (var obst in Obstacles)
                {
                    if (obst.Contains(p))
                    {
                        collision = true;
                        break;
                    }
                }

                if (!collision)
                {
                    AttrPoint = p;

                    AttrWeight = manip.DistanceTo(p) + manip.Goal.DistanceTo(p);
                    
                    r = 0.05 * Math.Pow(AttrWeight / manip.DistanceTo(manip.Goal), 4);
                    AttrArea = Primitives.Sphere(r, AttrPoint, 64);

                    manip.Attractors.Add(new Attractor(AttrPoint, AttrWeight, AttrArea, r));
                }
            }
            manip.States["Attractors"] = true;

            PathPlanner.RRT(rng, manip, Obstacles, new HillClimbing(Obstacles, manip.q.Length, AD.Precision, AD.StepSize, AD.MaxTime), AD.k, AD.d);
            //PathPlanner.RRT(rng, manip, Obstacles, new HillClimbing(Obstacles, manip.q.Length, 0.02, 3, 300), 10000, 0.08);
            //Tree.RectifyWhole();

            // retrieving resultant path along with respective configurations
            Tree.Node start = manip.Tree.Min(manip.Goal), node_curr = start;
            List<Point> path = new List<Point>();
            List<double[]> configs = new List<double[]>();
            for (int i = start.Layer; i >= 0; i--)
            {
                if (node_curr.Layer == i)
                {
                    path.Add(node_curr.p);
                    configs.Add(node_curr.q);
                    if (node_curr.Parent != null)
                    {
                        int pointsNum = node_curr.Layer - node_curr.Parent.Layer - 1;
                        if (pointsNum > 0)
                        {
                            Tree.Node[] nodes = Tree.Discretize(node_curr, node_curr.Parent, pointsNum);
                            foreach (var node in nodes)
                            {
                                configs.Add(node.q);
                            }
                        }
                    }

                    node_curr = node_curr.Parent;
                }
            }
            path.Reverse();
            configs.Reverse();

            manip.Path = path;
            manip.States["Path"] = true;
            
            for (int i = 0; i < configs.Count; i++)
            {                
                manip.q = configs[i];
                manip.Joints.Add(manip.DKP);
            }
        }
    }
}