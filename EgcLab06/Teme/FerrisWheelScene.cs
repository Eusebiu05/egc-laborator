// Varianta C: roata panoramică, inclusiv cabine cu resort amortizat.
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

public sealed class FerrisWheelScene : Scene
{
    private const string Faster = "crește viteza roții", Slower = "scade viteza roții", Stop = "oprește lin roata";
    private static readonly string[] Select = Enumerable.Range(1, 8).Select(i => $"adu cabina {i} la peron").ToArray();
    private readonly FerrisMotion _motion = new();
    private readonly SceneObject[] _cabins = new SceneObject[8];
    private readonly List<(SceneObject Part, int Index)> _panels = new();
    private SceneObject _hub = null!;
    private Mesh _cube = null!;
    private static readonly Color4[] Colors =
    {
        new(1,0.25f,0.25f,1), new(1,0.6f,0.15f,1), new(1,0.9f,0.2f,1), new(0.3f,0.85f,0.3f,1),
        new(0.2f,0.8f,0.95f,1), new(0.3f,0.4f,1,1), new(0.7f,0.3f,0.95f,1), new(1,0.4f,0.75f,1)
    };
    public override string Name => "Varianta C — roata panoramică";
    public override string Status => $"v {_motion.Speed:F1}°/s → {_motion.TargetSpeed:F0}°/s | " +
        (_motion.SelectedCabin < 0 ? "rotație liberă" : $"cabina {_motion.SelectedCabin + 1}: {(_motion.Docking ? "spre peron" : "la peron")}");
    public override IEnumerable<string> ActionNames => new[] { Faster, Slower, Stop }.Concat(Select);

    public override void Load(Input input)
    {
        input.Bind(Faster, Keys.Up);
        input.Bind(Slower, Keys.Down);
        input.Bind(Stop, Keys.Space);
        for (int i = 0; i < 8; i++) input.Bind(Select[i], (Keys)((int)Keys.D1 + i));
        CameraPosition = new Vector3(10, 8, 21);
        CameraTarget = new Vector3(0, 5.6f, 0);
        _cube = AddMesh(Shapes.ShadedCube());
        AddObject("podea", AddMesh(Shapes.Grid(12, 1, new Color4(0.28f, 0.3f, 0.36f, 1))));
        var steel = new Color4(0.65f, 0.72f, 0.85f, 1);
        foreach (float z in new[] { -0.8f, 0.8f })
        {
            Beam("suport stâng", null, new(-2.7f, 0, z), new(0, 6, z), 0.22f, steel);
            Beam("suport drept", null, new(2.7f, 0, z), new(0, 6, z), 0.22f, steel);
        }
        Box("peron", null, new(0, 0.25f, 0.7f), new(3.6f, 0.5f, 2.8f), new(0.3f, 0.5f, 0.65f, 1));
        _hub = AddObject("butuc pivot", null);
        _hub.Transform.Position = new Vector3(0, 6, 0);
        Box("ax central", _hub, Vector3.Zero, new(0.6f, 0.6f, 2.4f), steel);
        for (int i = 0; i < 64; i++)
        {
            float a = i * MathF.Tau / 64, b = (i + 1) * MathF.Tau / 64;
            Beam("jantă", _hub, new(4.5f * MathF.Cos(a), 4.5f * MathF.Sin(a), 0),
                new(4.5f * MathF.Cos(b), 4.5f * MathF.Sin(b), 0), 0.12f, steel);
        }
        for (int i = 0; i < 8; i++)
        {
            float a = i * MathF.Tau / 8;
            Vector3 pos = new(4.5f * MathF.Cos(a), 4.5f * MathF.Sin(a), 0);
            Beam("spiță", _hub, Vector3.Zero, pos, 0.12f, Colors[i]);
            var cabin = AddObject($"pivot cabină {i + 1}", null, _hub);
            cabin.Transform.Position = pos;
            _cabins[i] = cabin;
            Box("suspensie cabină", cabin, new(0, -0.18f, 0), new(0.08f, 0.36f, 0.08f), steel);
            var body = Box($"cabina {i + 1}", cabin, new(0, -0.65f, 0), new(1, 0.5f, 0.8f), Colors[i]);
            _panels.Add((body, i));
            Box("acoperiș", cabin, new(0, -0.3f, 0), new(1.1f, 0.1f, 0.9f), Colors[i]);
            foreach (float x in new[] { -0.45f, 0.45f })
                Box("montant", cabin, new(x, -0.45f, 0.3f), new(0.06f, 0.3f, 0.06f), steel);
            // Numărul cabinei este desenat geometric cu șapte segmente.
            AddDigit(i + 1, cabin);
        }
        ApplyMotion();
    }

    private SceneObject Box(string name, SceneObject? parent, Vector3 p, Vector3 scale, Color4 color)
    {
        var obj = AddObject(name, _cube, parent);
        obj.Transform.Position = p; obj.Transform.Scale = scale; obj.Tint = color;
        return obj;
    }

    private void Beam(string name, SceneObject? parent, Vector3 from, Vector3 to, float width, Color4 color)
    {
        Vector3 delta = to - from;
        var beam = Box(name, parent, (from + to) / 2, new(width, delta.Length, width), color);
        beam.Transform.Rotation.Z = -MathHelper.RadiansToDegrees(MathF.Atan2(delta.X, delta.Y));
    }

    private void AddDigit(int value, SceneObject parent)
    {
        string[] patterns = { "", "bc", "abdeg", "abcdg", "bcfg", "acdfg", "acdefg", "abc", "abcdefg" };
        var parts = new (char Id, float X, float Y, bool Horizontal)[]
        {
            ('a',0,0.15f,true), ('b',0.09f,0.075f,false), ('c',0.09f,-0.075f,false),
            ('d',0,-0.15f,true), ('e',-0.09f,-0.075f,false), ('f',-0.09f,0.075f,false), ('g',0,0,true)
        };
        foreach (var part in parts)
            if (patterns[value].Contains(part.Id))
                Box("număr cabină", parent, new(part.X, -0.65f + part.Y, 0.41f),
                    part.Horizontal ? new(0.18f,0.035f,0.02f) : new(0.035f,0.15f,0.02f), Color4.White);
    }

    public override void Update(float dt, Input input)
    {
        if (dt <= 0) return;
        if (input.WasPressed(Faster)) _motion.ChangeSpeed(5);
        if (input.WasPressed(Slower)) _motion.ChangeSpeed(-5);
        if (input.WasPressed(Stop)) _motion.Stop();
        for (int i = 0; i < 8; i++) if (input.WasPressed(Select[i])) _motion.SelectCabin(i);
        _motion.Update(dt);
        ApplyMotion();
    }

    private void ApplyMotion()
    {
        _hub.Transform.Rotation.Z = _motion.Angle;
        for (int i = 0; i < 8; i++)
            _cabins[i].Transform.Rotation.Z = -_motion.Angle + _motion.Swing[i];
        foreach (var panel in _panels)
            panel.Part.Tint = panel.Index == _motion.SelectedCabin ? Color4.White : Colors[panel.Index];
    }

    public override IEnumerable<string> Describe()
    {
        yield return Status;
        yield return "Sus/jos: viteza țintă ±5°/s (limite ±40°/s). Space: oprire lină.";
        yield return "1–8: cabina numerotată ajunge jos pe drumul cel mai scurt, cel mult 180° (Tween).";
        yield return "Bonus: cabinele au contrarotație și legănare cu resort amortizat la accelerare/frânare.";
        yield return $"Unghi: {_motion.Angle:F1}°; legănare maximă: {_motion.Swing.Max(x => MathF.Abs(x)):F2}°.";
    }
}
