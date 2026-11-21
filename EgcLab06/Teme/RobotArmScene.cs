// Varianta A: brațul robotic.
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

public sealed class RobotArmScene : Scene
{
    private const string BaseLeft = "bază spre stânga", BaseRight = "bază spre dreapta";
    private const string ShoulderDown = "coboară umărul", ShoulderUp = "ridică umărul";
    private const string ElbowDown = "îndoaie cotul", ElbowUp = "întinde cotul";
    private const string Grip = "deschide / închide cleștele", Save = "salvează poza";
    private const string Play = "pornește / oprește redarea", Ease = "schimbă easing-ul redării";
    private const string Clear = "șterge pozele";
    private const string ResetCargo = "reașază bila și brațul pentru prindere";
    private readonly KeyframeTrack _poses = new(1.5f) { Ease = Easing.EaseInOut };
    private readonly Tween _grip = new(new Vector3(0.55f), new Vector3(0.55f), 0.35f, Easing.EaseInOut);
    private SceneObject _base = null!, _shoulder = null!, _elbow = null!;
    private SceneObject _leftFinger = null!, _rightFinger = null!;
    private static readonly Vector3 StartAngles = new(0, -25, -100);
    private Vector3 _angles = StartAngles;
    private SceneObject _gripPoint = null!;
    private GrabbableBall _cargo = null!;
    private float _playTime;
    private bool _playing, _closed;
    private Tween? _entry;

    public override string Name => "Varianta A — braț robotic";
    public override string Status => $"{(_playing ? "redare" : "manual")} | poze: {_poses.Count} | " +
        $"unghiuri: {_angles.X:F0}/{_angles.Y:F0}/{_angles.Z:F0}° | " +
        $"{(_poses.Ease == Easing.Linear ? "Linear" : "EaseInOut")} | clește {(_closed ? "închis" : "deschis")} | {_cargo.State(_gripPoint)}";
    public override IEnumerable<string> ActionNames => new[]
    {
        BaseLeft, BaseRight, ShoulderDown, ShoulderUp, ElbowDown, ElbowUp, Grip, Save, Play, Ease, Clear, ResetCargo
    };

    public override void Load(Input input)
    {
        CameraPosition = new Vector3(9, 7, 12);
        CameraTarget = new Vector3(0, 2.8f, 0);
        input.Bind(BaseLeft, Keys.A, Keys.Left);
        input.Bind(BaseRight, Keys.D, Keys.Right);
        input.Bind(ShoulderDown, Keys.S, Keys.Down);
        input.Bind(ShoulderUp, Keys.W, Keys.Up);
        input.Bind(ElbowDown, Keys.Q);
        input.Bind(ElbowUp, Keys.Z);
        input.Bind(Grip, Keys.Space);
        input.Bind(Save, Keys.K);
        input.Bind(Play, Keys.Enter);
        input.Bind(Ease, Keys.E);
        input.Bind(Clear, Keys.Backspace);
        input.Bind(ResetCargo, Keys.R);

        Mesh cube = AddMesh(Shapes.ShadedCube());
        Mesh sphere = AddMesh(Shapes.ShadedSphere(1, 24, 12));
        AddObject("podea", AddMesh(Shapes.Grid(9, 1, new Color4(0.3f, 0.33f, 0.38f, 1))));
        var blue = new Color4(0.25f, 0.55f, 0.95f, 1);
        var gold = new Color4(1, 0.65f, 0.18f, 1);
        var steel = new Color4(0.65f, 0.72f, 0.8f, 1);
        Part("soclu", cube, null, new(0, 0.25f, 0), new(2, 0.5f, 2), steel);
        _base = AddObject("pivot bază", null);
        _base.Transform.Position = new Vector3(0, 0.5f, 0);
        Part("bază rotativă", cube, _base, new(0, 0.25f, 0), new(1.3f, 0.5f, 1.3f), blue);
        _shoulder = AddObject("pivot umăr", null, _base);
        _shoulder.Transform.Position = new Vector3(0, 0.6f, 0);
        Part("articulație umăr", sphere, _shoulder, Vector3.Zero, new(0.38f), steel);
        Part("segment superior", cube, _shoulder, new(0, 1.2f, 0), new(0.48f, 2.4f, 0.48f), gold);
        _elbow = AddObject("pivot cot", null, _shoulder);
        _elbow.Transform.Position = new Vector3(0, 2.4f, 0);
        Part("articulație cot", sphere, _elbow, Vector3.Zero, new(0.32f), steel);
        Part("antebraț", cube, _elbow, new(0, 1, 0), new(0.38f, 2, 0.38f), blue);
        SceneObject wrist = AddObject("pivot clește", null, _elbow);
        wrist.Transform.Position = new Vector3(0, 2, 0);
        Part("palmă", cube, wrist, Vector3.Zero, new(1.4f, 0.25f, 0.55f), steel);
        _leftFinger = Part("deget stâng", cube, wrist, Vector3.Zero, new(0.18f, 0.7f, 0.3f), gold);
        _rightFinger = Part("deget drept", cube, wrist, Vector3.Zero, new(0.18f, 0.7f, 0.3f), gold);
        _gripPoint = AddObject("centrul prizei", null, wrist);
        _gripPoint.Transform.Position = new Vector3(0, 0.4f, 0);
        ApplyPose();
        Vector3 start = _gripPoint.WorldPosition;
        float top = start.Y - GrabbableBall.Radius;
        Part("suport bilă", cube, null, new(start.X, top / 2, start.Z), new(1.1f, top, 1.1f), blue);
        SceneObject ball = Part("bilă de transportat", sphere, null, start,
            new Vector3(GrabbableBall.Radius), new Color4(0.3f, 1, 0.45f, 1));
        _cargo = new GrabbableBall(ball, start);
    }

    private SceneObject Part(string name, Mesh mesh, SceneObject? parent, Vector3 position, Vector3 scale, Color4 tint)
    {
        SceneObject part = AddObject(name, mesh, parent);
        part.Transform.Position = position;
        part.Transform.Scale = scale;
        part.Tint = tint;
        return part;
    }

    public override void Update(float dt, Input input)
    {
        // Inclusiv comenzile care ar schimba poza sunt suspendate în pauză.
        if (dt <= 0) return;
        if (input.WasPressed(ResetCargo))
        {
            _playing = false;
            _entry = null;
            _angles = StartAngles;
            _closed = false;
            _grip.From = _grip.To = new Vector3(0.55f);
            _grip.Restart();
            _cargo.Reset();
            ApplyPose();
            return;
        }
        if (input.WasPressed(Clear))
        {
            _poses.Clear();
            _playing = false;
            _entry = null;
            _playTime = 0;
        }
        if (input.WasPressed(Save))
        {
            _poses.Add(_angles);
            ConsolePanel.PrintMessage($"Poza {_poses.Count} salvată: {_angles}");
        }
        if (input.WasPressed(Ease))
            _poses.Ease = _poses.Ease == Easing.Linear ? Easing.EaseInOut : Easing.Linear;
        if (input.WasPressed(Play))
        {
            if (_playing) { _playing = false; _entry = null; }
            else if (_poses.Count > 0)
            {
                _playing = true;
                _playTime = 0;
                // Intrare lină în prima poză, fără salt din poziția manuală.
                _entry = new Tween(_angles, _poses.Keys[0], 0.5f, Easing.EaseInOut);
            }
            else ConsolePanel.PrintMessage("Salvează întâi o poză cu K.");
        }
        if (input.WasPressed(Grip))
        {
            _closed = !_closed;
            _grip.GoTo(new Vector3(_closed ? 0.32f : 0.55f), 0.35f);
        }
        if (_playing)
        {
            if (_entry != null)
            {
                float used = MathF.Min(dt, _entry.Duration - _entry.Elapsed);
                _entry.Update(used);
                _angles = _entry.Value;
                if (_entry.IsFinished)
                {
                    _entry = null;
                    _playTime += dt - used;
                    _angles = _poses.Sample(_playTime);
                }
            }
            else
            {
                _playTime = (_playTime + dt) % _poses.Duration;
                _angles = _poses.Sample(_playTime);
            }
        }
        else
        {
            _angles.X += input.Axis(BaseLeft, BaseRight) * 65 * dt;
            _angles.Y = Math.Clamp(_angles.Y + input.Axis(ShoulderDown, ShoulderUp) * 50 * dt, -75, 75);
            _angles.Z = Math.Clamp(_angles.Z + input.Axis(ElbowDown, ElbowUp) * 65 * dt, -135, 20);
        }
        _grip.Update(dt);
        ApplyPose();
        _cargo.Update(dt, _gripPoint, _closed, _grip.Value.X);
    }

    private void ApplyPose()
    {
        _base.Transform.Rotation = new Vector3(0, _angles.X, 0);
        _shoulder.Transform.Rotation = new Vector3(0, 0, _angles.Y);
        _elbow.Transform.Rotation = new Vector3(0, 0, _angles.Z);
        _leftFinger.Transform.Position = new Vector3(-_grip.Value.X, 0.4f, 0);
        _rightFinger.Transform.Position = new Vector3(_grip.Value.X, 0.4f, 0);
    }

    public override IEnumerable<string> Describe()
    {
        yield return Status;
        yield return "Bază: A/D sau stânga/dreapta (65°/s); umăr: S/W sau jos/sus (50°/s).";
        yield return "Cot: Q/Z (65°/s). Limite: umăr [-75°, 75°], cot [-135°, 20°].";
        yield return "Space: clește (0,35 s). K: salvează; Enter: redă/oprește; E: easing; Backspace: șterge.";
        yield return "Bonus: Space prinde bila verde dintre degete; deschide cleștele pentru eliberare.";
        yield return "R reașază bila pe suport și brațul în poziția de prindere. P îngheață și căderea.";
        yield return $"Durata buclei: {_poses.Duration:F1} s; 1,5 s între poze. Intrare în redare: 0,5 s.";
        for (int i = 0; i < _poses.Count; i++)
            yield return $"Poza {i + 1}: {_poses.Keys[i]}";
    }
}
