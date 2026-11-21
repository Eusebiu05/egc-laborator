using OpenTK.Mathematics;

namespace Egc;

public sealed class FerrisMotion
{
    private Tween? _dock;
    public float Angle { get; private set; }
    public float Speed { get; private set; } = 12;
    public float TargetSpeed { get; private set; } = 12;
    public int SelectedCabin { get; private set; } = -1;
    public bool Docking => _dock != null;
    public readonly float[] Swing = new float[8];
    private readonly float[] _swingSpeed = new float[8];

    public static float ShortestDelta(float from, float to) => ((to - from) % 360 + 540) % 360 - 180;

    public void ChangeSpeed(float delta)
    {
        _dock = null;
        SelectedCabin = -1;
        TargetSpeed = Math.Clamp(TargetSpeed + delta, -40, 40);
    }

    public void Stop()
    {
        _dock = null;
        SelectedCabin = -1;
        TargetSpeed = 0;
    }

    public void SelectCabin(int index)
    {
        if (index < 0 || index >= 8) throw new ArgumentOutOfRangeException(nameof(index));
        SelectedCabin = index;
        TargetSpeed = 0;
        float delta = ShortestDelta(Angle, -90 - index * 45);
        _dock = new Tween(new Vector3(Angle, 0, 0), new Vector3(Angle + delta, 0, 0),
            MathF.Max(1, MathF.Abs(delta) / 45), Easing.EaseInOut);
    }

    public void Update(float dt)
    {
        if (dt <= 0) return;
        int steps = Math.Max(1, (int)MathF.Ceiling(dt / 0.005f));
        float h = dt / steps;
        for (int s = 0; s < steps; s++)
        {
            float previousSpeed = Speed;
            if (_dock != null)
            {
                float oldAngle = Angle;
                _dock.Update(h);
                Angle = _dock.Value.X;
                Speed = (Angle - oldAngle) / h;
                if (_dock.IsFinished) { _dock = null; Speed = 0; }
            }
            else
            {
                float decay = MathF.Exp(-2.5f * h);
                // Integrarea exactă a vitezei exponențiale: aceeași evoluție la orice FPS.
                Angle += TargetSpeed * h + (Speed - TargetSpeed) * (1 - decay) / 2.5f;
                Speed = TargetSpeed + (Speed - TargetSpeed) * decay;
            }
            float acceleration = Math.Clamp((Speed - previousSpeed) / h, -120, 120);
            for (int i = 0; i < 8; i++)
            {
                float phase = MathHelper.DegreesToRadians(Angle + i * 45);
                float force = -0.18f * acceleration * MathF.Sin(phase);
                _swingSpeed[i] += (force - 12 * Swing[i] - 3.5f * _swingSpeed[i]) * h;
                Swing[i] += _swingSpeed[i] * h;
            }
        }
        if (_dock == null) Angle %= 360;
    }
}
