using OpenTK.Mathematics;

namespace Egc;

/// <summary>Conducerea mașinii prin acțiuni, cu accelerație, direcție și saltul încărcat din P4.</summary>
public sealed class PlayerController
{
    public const float Speed = 6.0f;          // unități pe secundă, în plan
    public const float Gravity = 20.0f;       // unități / s²
    public const float MinJumpSpeed = 4.0f;   // viteza verticală la o apăsare scurtă (salt mic)
    public const float MaxJumpSpeed = 11.0f;  // viteza verticală după încărcarea completă
    public const float ChargeTime = 0.8f;     // secunde de ținere pentru încărcarea completă

    private readonly SceneObject _body;
    private readonly SceneObject _shadow;
    private readonly Vector3 _start;
    private readonly float _groundY;          // înălțimea pivotului mașinii pe podea
    private readonly Vector2 _min, _max;      // limitele zonei de joc, în planul XZ

    private Vector3 _position;                // poziția "logică" (fără turtirea vizuală)
    private float _verticalSpeed;             // viteza pe Y (pozitivă = în sus)
    private bool _charging;                   // tasta de salt este ținută, pe sol
    private float _heading = 180;
    public float DriveSpeed { get; private set; }
    public float SteeringAngle { get; private set; }
    private bool _pendingJump;
    private float _charge;                    // 0 .. 1

    public PlayerController(SceneObject body, SceneObject shadow, Vector2 min, Vector2 max)
    {
        _body = body;
        _shadow = shadow;
        _start = body.Transform.Position;
        _position = _start;
        _groundY = _start.Y;
        _min = min;
        _max = max;
    }

    public bool OnGround => _position.Y <= _groundY + 1e-4f;
    public Vector3 Position => _position;
    public float Charge => _charge;
    public float VerticalSpeed => _verticalSpeed;

    public void Update(float dt, Input input)
    {
        // Memorează eliberarea în pauză, fără a modifica poziția sau încărcarea.
        if (dt <= 0)
        {
            if (_charging && input.WasReleased(Actions.Jump)) _pendingJump = true;
            return;
        }
        Vector3 p = _position;
        float throttle = input.Axis(Actions.Back, Actions.Forward);
        float steer = input.Axis(Actions.Right, Actions.Left);
        float targetSpeed = throttle * (throttle < 0 ? Speed * 0.5f : Speed);
        // Subpași pentru stabilitate inclusiv la scara timpului x4.
        int steps = Math.Max(1, (int)MathF.Ceiling(dt / 0.01f));
        float step = dt / steps;
        for (int i = 0; i < steps; i++)
        {
            float acceleration = throttle == 0 ? 5.0f : 8.0f;
            DriveSpeed += Math.Clamp(targetSpeed - DriveSpeed, -acceleration * step, acceleration * step);
            SteeringAngle += (steer * 28 - SteeringAngle) * (1 - MathF.Exp(-10 * step));
            if (OnGround)
                _heading += MathHelper.RadiansToDegrees(DriveSpeed / 2.36f * MathF.Tan(MathHelper.DegreesToRadians(SteeringAngle))) * step;
            float radians = MathHelper.DegreesToRadians(_heading);
            float x = p.X + MathF.Sin(radians) * DriveSpeed * step;
            float z = p.Z + MathF.Cos(radians) * DriveSpeed * step;
            p.X = Math.Clamp(x, _min.X, _max.X);
            p.Z = Math.Clamp(z, _min.Y, _max.Y);
            if (p.X != x || p.Z != z) DriveSpeed = 0;
        }
        _heading %= 360;
        _body.Transform.Rotation.Y = _heading;

        // 2. Saltul încărcat: apăsare -> ținere -> eliberare.
        // Rezolvare 6.5:
        //   (a) pe sol, la APĂSAREA acțiunii Jump: _charging = true și _charge = 0;
        //   (b) cât timp _charging și acțiunea este ȚINUTĂ: _charge crește cu dt / ChargeTime, cel mult 1;
        //   (c) la ELIBERARE (dacă _charging): _verticalSpeed = Lerp(MinJumpSpeed, MaxJumpSpeed, _charge),
        //       apoi _charging = false și _charge = 0.

        if (OnGround && input.WasPressed(Actions.Jump))
        {
            _charging = true;
            _charge = 0.0f;
        }
        if (_charging && input.IsDown(Actions.Jump))
            _charge = MathF.Min(1.0f, _charge + dt / ChargeTime);
        if (_charging && (input.WasReleased(Actions.Jump) || _pendingJump))
        {
            _pendingJump = false;
            _verticalSpeed = Easing.Lerp(MinJumpSpeed, MaxJumpSpeed, _charge);
            _charging = false;
            _charge = 0.0f;
        }

        // 3. Gravitația (metoda Euler semi-implicită): întâi viteza, apoi poziția.
        if (!OnGround || _verticalSpeed > 0.0f)
        {
            _verticalSpeed -= Gravity * dt;
            p.Y += _verticalSpeed * dt;
            if (p.Y <= _groundY)
            {
                p.Y = _groundY;          // a aterizat
                _verticalSpeed = 0.0f;
            }
        }
        _position = p;

        // 4. Suspensia se comprimă la încărcarea saltului;
        //    umbra rămâne pe podea și se micșorează cu înălțimea.
        float squash = 0.35f * _charge;
        _body.Transform.Scale = new Vector3(1.0f + 0.5f * squash, 1.0f - squash, 1.0f + 0.5f * squash);
        _body.Transform.Position = p;

        float height = p.Y - _groundY;
        float shadowSize = 1.1f / (1.0f + 0.25f * height);
        _shadow.Transform.Position = new Vector3(p.X, 0.01f, p.Z);
        _shadow.Transform.Rotation.Y = _heading;
        _shadow.Transform.Scale = new Vector3(shadowSize * 1.8f, 1.0f, shadowSize * 3.6f);
    }

    public void Reset()
    {
        DriveSpeed = 0;
        SteeringAngle = 0;
        _heading = 180;
        _pendingJump = false;
        _body.Transform.Scale = Vector3.One;
        _body.Transform.Rotation.Y = _heading;
        _position = _start;
        _body.Transform.Position = _start;
        _verticalSpeed = 0.0f;
        _charging = false;
        _charge = 0.0f;
    }

    public string State => _charging ? "încarcă saltul" : OnGround ? "pe sol" : "în aer";
}
