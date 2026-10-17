using System;
using Microsoft.Xna.Framework;

using MonoGameLibrary.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class Camera3D
    {
        public Vector3 Target = Vector3.Zero;
        public float Yaw = 0f;
        public float Pitch = MathHelper.ToRadians(35f);
        public float Distance = 12f;

        public float FOV = MathHelper.ToRadians(35f);
        public float AspectRatio = 16f / 9f;
        public float NearPlane = 0.1f;
        public float FarPlane = 500f;

        public Vector3 Pos => Target + new Vector3(MathF.Cos(Pitch) * MathF.Sin(Yaw),
                                                   MathF.Sin(Pitch),
                                                   MathF.Cos(Pitch)*MathF.Cos(Yaw)) * Distance;

        public Matrix View => Matrix.CreateLookAt(Pos, Target, Vector3.Up);
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearPlane, FarPlane);
        public Vector3 Forward => new Vector3(-MathF.Sin(Yaw), 0f, -MathF.Cos(Yaw));
        public Vector3 Right => Vector3.Cross(Forward, Vector3.Up);  

        public void ClampPitch()
        {
            Pitch = MathHelper.Clamp(Pitch, MathHelper.ToRadians(10f), MathHelper.ToRadians(80f));
        }
    }
}
