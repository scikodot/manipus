﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Logic.InverseKinematics;

namespace Logic.PathPlanning
{
    public enum PathPlannerType
    {
        DynamicRRT,
        GeneticAlgorithm
    }

    public struct PathPlanningData
    {
        public int PathPlannerID;
        public int AttrNum;
        public int k;
        public float d;
    }

    public abstract class PathPlanner
    {
        public static string[] Types { get; } = Enum.GetNames(typeof(PathPlannerType));

        protected static Random Rng = new Random();

        protected bool CollisionCheck;

        private int _maxTime;
        public ref int MaxTime => ref _maxTime;

        protected PathPlanner(int maxTime, bool collisionCheck)
        {
            MaxTime = maxTime;
            CollisionCheck = collisionCheck;
        }

        public abstract (List<Vector3>, List<Vector>) Execute(Obstacle[] Obstacles, Manipulator agent, Vector3 goal, InverseKinematicsSolver Solver);
    }
}