using OpenTK.Mathematics;

namespace Egc;

/// <summary>Obiect transportabil: atașare la clește, eliberare în lume și cădere pe suport/sol.</summary>
public sealed class GrabbableBall
{
    public const float Radius = 0.23f;
    public const float CaptureDistance = 0.28f;
    private readonly SceneObject _ball;
    private readonly Vector3 _start;
    private float _fallSpeed;
    public bool Held => _ball.Parent != null;

    public GrabbableBall(SceneObject ball, Vector3 start)
    {
        _ball = ball;
        _start = start;
    }

    public string State(SceneObject grip) => Held ? "bilă prinsă" :
        (_ball.WorldPosition - grip.WorldPosition).Length <= CaptureDistance
        ? "bilă în raza cleștelui — Space" : "bilă liberă — R pentru reset";

    public void Update(float dt, SceneObject grip, bool closing, float opening)
    {
        if (dt <= 0) return;
        if (Held)
        {
            if (!closing && opening >= 0.43f)
            {
                // Salvează poziția mondială înainte de a schimba părintele.
                Vector3 world = _ball.WorldPosition;
                _ball.Parent = null;
                _ball.Transform.Position = world;
                _fallSpeed = 0;
            }
            else return;
        }
        else if (closing && opening <= 0.34f &&
                 (_ball.WorldPosition - grip.WorldPosition).Length <= CaptureDistance)
        {
            _ball.Parent = grip;
            _ball.Transform.Position = Vector3.Zero;
            _fallSpeed = 0;
            return;
        }

        // Subpași pentru cădere stabilă la orice scară a timpului de joc.
        int steps = Math.Max(1, (int)MathF.Ceiling(dt / 0.01f));
        float step = dt / steps;
        for (int i = 0; i < steps; i++)
        {
            Vector3 p = _ball.Transform.Position;
            float floor = Radius;
            bool aboveStand = MathF.Abs(p.X - _start.X) <= 0.55f && MathF.Abs(p.Z - _start.Z) <= 0.55f;
            if (aboveStand && p.Y >= _start.Y - 0.001f) floor = _start.Y;
            _fallSpeed -= 9.81f * step;
            p.Y += _fallSpeed * step;
            if (p.Y <= floor)
            {
                p.Y = floor;
                _fallSpeed = 0;
            }
            _ball.Transform.Position = p;
        }
    }

    public void Reset()
    {
        _ball.Parent = null;
        _ball.Transform.Position = _start;
        _fallSpeed = 0;
    }
}
