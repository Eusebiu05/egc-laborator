// Varianta B: evită obstacolele, inclusiv marcajele rutiere în mișcare.
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

public sealed class RunnerScene : Scene
{
    private const string Left = "banda din stânga", Right = "banda din dreapta";
    private const string Jump = "salt peste obstacol", Restart = "joc nou";
    private readonly RunnerSimulation _game = new();
    private readonly List<SceneObject> _obstacleViews = new();
    private readonly List<(SceneObject Object, float Z)> _marks = new();
    private SceneObject _player = null!;
    private Mesh _cube = null!;
    public override string Name => "Varianta B — evită obstacolele";
    public override string Status => $"{(_game.GameOver ? "ACCIDENT — R: joc nou" : "în curs")} | scor {_game.Score:F1}s | record {_game.Record:F1}s | v {_game.Speed:F1}";
    public override IEnumerable<string> ActionNames => new[] { Left, Right, Jump, Restart };

    public override void Load(Input input)
    {
        input.Bind(Left, Keys.Left, Keys.A);
        input.Bind(Right, Keys.Right, Keys.D);
        input.Bind(Jump, Keys.Space);
        input.Bind(Restart, Keys.R);
        CameraPosition = new Vector3(0, 7, 11);
        CameraTarget = new Vector3(0, 0, -12);
        _cube = AddMesh(Shapes.ShadedCube());
        Box("drum", null, new(0, -0.15f, -15), new(8, 0.3f, 44), new(0.15f, 0.17f, 0.21f, 1));
        foreach (float x in new[] { -4.1f, 4.1f })
            Box("margine drum", null, new(x, 0.1f, -15), new(0.15f, 0.2f, 44), new(0.95f, 0.35f, 0.12f, 1));
        foreach (float x in new[] { -1.25f, 1.25f })
            for (int i = 0; i < 15; i++)
            {
                float z = -38 + i * 3;
                var mark = Box("marcaj", null, new(x, 0.015f, z), new(0.08f, 0.025f, 1.5f), Color4.White);
                _marks.Add((mark, z));
            }
        _player = AddObject("pivot mașină", null);
        Box("caroserie", _player, new(0, -0.1f, 0), new(1.1f, 0.5f, 1.8f), new(0.95f, 0.06f, 0.08f, 1));
        Box("cabină", _player, new(0, 0.25f, 0.15f), new(0.8f, 0.4f, 0.8f), new(0.2f, 0.65f, 0.85f, 1));
        foreach (float x in new[] { -0.48f, 0.48f })
            foreach (float z in new[] { -0.55f, 0.55f })
                Box("roată", _player, new(x, -0.25f, z), new(0.2f, 0.4f, 0.4f), new(0.04f, 0.04f, 0.05f, 1));
        SyncViews();
    }

    private SceneObject Box(string name, SceneObject? parent, Vector3 p, Vector3 scale, Color4 color)
    {
        var obj = AddObject(name, _cube, parent);
        obj.Transform.Position = p;
        obj.Transform.Scale = scale;
        obj.Tint = color;
        return obj;
    }

    public override void Update(float dt, Input input)
    {
        if (dt <= 0) return;
        if (input.WasPressed(Restart)) _game.Reset();
        int direction = (input.WasPressed(Right) ? 1 : 0) - (input.WasPressed(Left) ? 1 : 0);
        if (direction != 0) _game.ChangeLane(direction);
        if (input.WasPressed(Jump)) _game.Jump();
        _game.Update(dt);
        SyncViews();
    }

    private void SyncViews()
    {
        _player.Transform.Position = _game.Position;
        while (_obstacleViews.Count < _game.Obstacles.Count)
            _obstacleViews.Add(AddObject("obstacol", _cube));
        for (int i = 0; i < _obstacleViews.Count; i++)
        {
            var view = _obstacleViews[i];
            view.Visible = i < _game.Obstacles.Count;
            if (!view.Visible) continue;
            var obstacle = _game.Obstacles[i];
            view.Transform.Position = obstacle.Position;
            view.Transform.Scale = obstacle.Size;
            view.Tint = obstacle.Size.Y < 1 ? new Color4(1, 0.75f, 0.12f, 1) : new Color4(0.65f, 0.25f, 0.95f, 1);
        }
        foreach (var mark in _marks)
            mark.Object.Transform.Position.Z = -38 + ((mark.Z + 38 + _game.Distance) % 45);
    }

    public override IEnumerable<string> Describe()
    {
        yield return Status;
        yield return "A/D sau săgeți: schimbare bandă în 0,16 s (EaseOut). Space: salt. R: joc nou.";
        yield return "Galben: obstacol jos, poate fi sărit. Violet: obstacol înalt, trebuie ocolit.";
        yield return $"Bandă: {_game.Lane + 2}/3; interval obstacole: {_game.SpawnInterval:F2}s; coliziuni AABB.";
        yield return "Bonus: marcajele se deplasează cu viteza lumii. Recordul se păstrează între jocuri în această sesiune.";
    }
}
