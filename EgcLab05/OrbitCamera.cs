using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

/// <summary>Cameră care se deplasează pe o sferă în jurul țintei.</summary>
public class OrbitCamera : Camera
{
    public Vector3 Target { get; private set; }
    public float Distance { get; private set; }
    public float Azimuth { get; private set; }
    public float Elevation { get; private set; }
    public const float MinDistance = 0.5f;
    public const float MaxDistance = 50f;
    public const float MaxElevation = 89f;
    public float MouseSensitivity = 0.2f;
    public float RotationSpeed = 60f; // grade pe secundă
    public float MoveSpeed = 3f;

    public OrbitCamera() => Reset();
    public override string Name => "Orbită";

    public override Vector3 Position
    {
        get
        {
            float az = MathHelper.DegreesToRadians(Azimuth);
            float el = MathHelper.DegreesToRadians(Elevation);
            return Target + Distance * new Vector3(MathF.Cos(el) * MathF.Cos(az),
                MathF.Sin(el), MathF.Cos(el) * MathF.Sin(az));
        }
    }

    public override Vector3 Front => Vector3.Normalize(Target - Position);
    public override Matrix4 ViewMatrix => Matrix4.LookAt(Position, Target, Vector3.UnitY);

    public override void Update(KeyboardState keyboard, MouseState mouse, Vector2 mouseDelta, float dt)
    {
        if (mouse.IsButtonDown(MouseButton.Left))
        {
            Azimuth -= mouseDelta.X * MouseSensitivity;
            Elevation += mouseDelta.Y * MouseSensitivity;
        }
        if (keyboard.IsKeyDown(Keys.Left)) Azimuth += RotationSpeed * dt;
        if (keyboard.IsKeyDown(Keys.Right)) Azimuth -= RotationSpeed * dt;
        if (keyboard.IsKeyDown(Keys.Up)) Elevation += RotationSpeed * dt;
        if (keyboard.IsKeyDown(Keys.Down)) Elevation -= RotationSpeed * dt;
        Azimuth %= 360f;
        Elevation = Math.Clamp(Elevation, -MaxElevation, MaxElevation);
        // Zoom prin deplasare pe rază; FOV-ul rămâne neschimbat.
        Distance = Math.Clamp(Distance - mouse.ScrollDelta.Y * 0.5f, MinDistance, MaxDistance);

        Vector3 forward = Vector3.Normalize(new Vector3(Front.X, 0, Front.Z));
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        Vector3 move = Vector3.Zero;
        if (keyboard.IsKeyDown(Keys.W)) move += forward;
        if (keyboard.IsKeyDown(Keys.S)) move -= forward;
        if (keyboard.IsKeyDown(Keys.D)) move += right;
        if (keyboard.IsKeyDown(Keys.A)) move -= right;
        float speed = MoveSpeed * (keyboard.IsKeyDown(Keys.LeftShift) ? 2.5f : 1f);
        if (move.LengthSquared > 0) Target += Vector3.Normalize(move) * speed * dt;
    }

    /// <summary>Alege ținta în fața camerei FPS și păstrează poziția, privirea și proiecția.</summary>
    public void MatchView(FpsCamera camera)
    {
        Distance = 5f;
        Target = camera.Position + camera.Front * Distance;
        Vector3 offset = -camera.Front;
        Elevation = Math.Clamp(MathHelper.RadiansToDegrees(MathF.Asin(Math.Clamp(offset.Y, -1f, 1f))),
            -MaxElevation, MaxElevation);
        Azimuth = MathHelper.RadiansToDegrees(MathF.Atan2(offset.Z, offset.X));
        FieldOfView = camera.FieldOfView;
        Near = camera.Near;
        Far = camera.Far;
    }

    public override void Reset()
    {
        Target = new Vector3(0, 1.2f, -2f); // cubul central
        Distance = 8f;
        Azimuth = 90f;
        Elevation = 20f;
        FieldOfView = 60f;
    }

    public override IEnumerable<string> Describe()
    {
        yield return $"Țintă          : ({Target.X:F2}, {Target.Y:F2}, {Target.Z:F2})";
        yield return $"Distanță       : {Distance:F2}";
        yield return $"Azimut/elevație : {Azimuth:F1}° / {Elevation:F1}°";
        yield return $"Poziție        : ({Position.X:F2}, {Position.Y:F2}, {Position.Z:F2})";
    }
}
