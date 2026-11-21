using OpenTK.Mathematics;

namespace Egc;

public sealed class RunnerSimulation
{
    public sealed class Obstacle
    {
        public Vector3 Position;
        public Vector3 Size;
    }
    public readonly List<Obstacle> Obstacles = new();
    private readonly Random _random = new(606);
    private Tween _laneTween = new(Vector3.Zero, Vector3.Zero, 0.16f, Easing.EaseOut);
    private float _spawnTimer = 1.2f, _verticalSpeed;
    public int Lane { get; private set; }
    public Vector3 Position { get; private set; }
    public static readonly Vector3 PlayerSize = new(1.1f, 0.9f, 1.8f);
    public float Score { get; private set; }
    public float Record { get; private set; }
    public float Distance { get; private set; }
    public float Speed => MathF.Min(22, 7 + Score * 0.12f);
    public float SpawnInterval => MathF.Max(0.65f, 1.6f - Score * 0.008f);
    public bool GameOver { get; private set; }

    public void Reset()
    {
        Obstacles.Clear();
        Lane = 0;
        Position = new Vector3(0, PlayerSize.Y / 2, 0);
        _laneTween = new Tween(Vector3.Zero, Vector3.Zero, 0.16f, Easing.EaseOut);
        _spawnTimer = 1.2f;
        _verticalSpeed = Score = Distance = 0;
        GameOver = false;
    }

    public RunnerSimulation() => Reset();

    public void ChangeLane(int direction)
    {
        if (GameOver) return;
        Lane = Math.Clamp(Lane + direction, -1, 1);
        _laneTween.GoTo(new Vector3(Lane * 2.5f, 0, 0), 0.16f);
    }

    public void Jump()
    {
        if (!GameOver && Position.Y <= PlayerSize.Y / 2 + 0.001f) _verticalSpeed = 8;
    }

    public static bool Overlaps(Vector3 a, Vector3 sizeA, Vector3 b, Vector3 sizeB) =>
        MathF.Abs(a.X - b.X) < (sizeA.X + sizeB.X) / 2 &&
        MathF.Abs(a.Y - b.Y) < (sizeA.Y + sizeB.Y) / 2 &&
        MathF.Abs(a.Z - b.Z) < (sizeA.Z + sizeB.Z) / 2;

    public void Update(float dt)
    {
        if (dt <= 0 || GameOver) return;
        int steps = Math.Max(1, (int)MathF.Ceiling(dt / 0.005f));
        float h = dt / steps;
        for (int i = 0; i < steps && !GameOver; i++)
        {
            _laneTween.Update(h);
            _verticalSpeed -= 20 * h;
            float y = Position.Y + _verticalSpeed * h;
            if (y < PlayerSize.Y / 2) { y = PlayerSize.Y / 2; _verticalSpeed = 0; }
            Position = new Vector3(_laneTween.Value.X, y, 0);
            float travel = Speed * h;
            Distance += travel;
            Score += h;
            Record = MathF.Max(Record, Score);
            _spawnTimer -= h;
            if (_spawnTimer <= 0)
            {
                _spawnTimer += SpawnInterval;
                float height = _random.Next(2) == 0 ? 0.65f : 3.5f;
                Obstacles.Add(new Obstacle
                {
                    Position = new Vector3(_random.Next(-1, 2) * 2.5f, height / 2, -35),
                    Size = new Vector3(1.8f, height, 1.0f)
                });
            }
            foreach (Obstacle obstacle in Obstacles)
            {
                obstacle.Position.Z += travel;
                if (Overlaps(Position, PlayerSize, obstacle.Position, obstacle.Size)) GameOver = true;
            }
            Obstacles.RemoveAll(o => o.Position.Z > 6);
        }
    }
}
