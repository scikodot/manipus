﻿using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Logic.InverseKinematics
{
    public class DampedLeastSquares : InverseKinematicsSolver
    {
        private float _lambda = 0.5f;
        public ref float Lambda => ref _lambda;

        public DampedLeastSquares(float precision, float stepSize, int maxTime) : base(precision, stepSize, maxTime) { }

        public override (bool, float, Vector, bool[]) Execute(Obstacle[] Obstacles, Manipulator agent, Vector3 goal, int joint)
        {
            Vector initConfig = agent.q;
            MathNet.Numerics.LinearAlgebra.Vector<float> dq;
            for (int j = 0; j < 4; j++)
            {
                Vector3 jointPos = agent.Joints[joint].Position;
                Vector3 error = goal - jointPos;  // TODO: check for oscillations (the error starts increasing) and break if they appear
                var errorExt = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.Dense(new float[]
                {
                    error.X,
                    error.Y,
                    error.Z,
                    0, 0, 0
                });

                float[][] data = new float[joint + 1][];
                for (int i = 0; i <= joint; i++)
                {
                    var elem = Vector3.Cross(agent.Joints[i].Axis, jointPos - agent.Joints[i].Position);
                    if (elem != Vector3.Zero)
                        elem = Vector3.Normalize(elem);
                    data[i] = new float[]
                    {
                        elem.X,
                        elem.Y,
                        elem.Z,
                        agent.Joints[i].Axis.X,
                        agent.Joints[i].Axis.Y,
                        agent.Joints[i].Axis.Z
                    };
                }

                // get Jacobian and its transpose
                var J = Matrix<float>.Build.DenseOfColumnArrays(data);
                var JT = Matrix<float>.Build.DenseOfRowArrays(data);
                var core = J * JT + _lambda * _lambda * Matrix<float>.Build.DenseIdentity(errorExt.Count);
                var f = core.Solve(errorExt);
                dq = -JT * f;

                Vector dqLocal = new Vector(dq.Storage.AsArray());
                if (joint < agent.Joints.Length - 1)
                    dqLocal.Expand(agent.Joints.Length - joint);

                agent.q += dqLocal;
            }

            // checking for collisions of the found configuration
            bool[] collisions = DetectCollisions(agent, Obstacles);
            var dist = agent.Joints[joint].Position.DistanceTo(goal);

            return (true, dist, agent.q - initConfig, collisions);
        }
    }
}