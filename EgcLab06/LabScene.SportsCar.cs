using OpenTK.Mathematics;

namespace Egc;

public sealed partial class LabScene
{
    // Toate piesele urmează același pivot; scalările rămân pe formele copil.
    private readonly List<(SceneObject Steering, SceneObject Rolling, bool Front)> _carWheels = new();
    private float _wheelAngle;

    private SceneObject BuildSportsCar(Mesh cube)
    {
        SceneObject car = AddObject("mașină sport", null);
        car.Transform.Position = new Vector3(0, 0, 2.2f);
        car.Transform.Rotation.Y = 180;
        Mesh tire = AddMesh(Shapes.ShadedCylinder(0.38f, 0.26f, 32));
        Mesh rim = AddMesh(Shapes.ShadedCylinder(0.25f, 0.28f, 24));
        Color4 red = new(0.95f, 0.045f, 0.075f, 1);
        Color4 dark = new(0.055f, 0.065f, 0.08f, 1);
        Color4 glass = new(0.16f, 0.42f, 0.57f, 1);
        Color4 silver = new(0.8f, 0.85f, 0.9f, 1);

        SceneObject Part(string name, Mesh mesh, Vector3 position, Vector3 scale, Color4 tint)
        {
            SceneObject obj = AddObject(name, mesh, car);
            obj.Transform.Position = position;
            obj.Transform.Scale = scale;
            obj.Tint = tint;
            return obj;
        }
        void Box(string name, Vector3 position, Vector3 scale, Color4 tint) =>
            Part(name, cube, position, scale, tint);

        Box("șasiu sport", new(0, 0.43f, 0), new(1.85f, 0.35f, 3.8f), dark);
        Box("caroserie roșie", new(0, 0.65f, 0), new(1.9f, 0.38f, 3.65f), red);
        SceneObject hood = Part("capotă înclinată", cube, new(0, 0.85f, 1.1f), new(1.8f, 0.16f, 1.35f), red);
        hood.Transform.Rotation.X = 7;
        Box("habitaclu vitrat", new(0, 1.02f, -0.25f), new(1.48f, 0.48f, 1.5f), glass);
        Box("plafon", new(0, 1.29f, -0.4f), new(1.52f, 0.1f, 1.1f), red);
        SceneObject windshield = Part("parbriz", cube, new(0, 1.07f, 0.51f), new(1.45f, 0.48f, 0.07f), glass);
        windshield.Transform.Rotation.X = 28;
        Box("splitter frontal", new(0, 0.34f, 1.93f), new(2.02f, 0.09f, 0.26f), dark);
        Box("grilă frontală", new(0, 0.56f, 1.84f), new(0.98f, 0.19f, 0.035f), dark);
        Box("difuzor spate", new(0, 0.4f, -1.88f), new(1.7f, 0.17f, 0.12f), dark);
        foreach (float side in new[] { -1.0f, 1.0f })
        {
            Box("far LED", new(side * 0.66f, 0.73f, 1.84f), new(0.43f, 0.12f, 0.04f), new Color4(0.85f, 0.97f, 1, 1));
            Box("stop", new(side * 0.65f, 0.73f, -1.84f), new(0.46f, 0.1f, 0.04f), new Color4(1, 0.08f, 0.02f, 1));
            Box("oglindă", new(side * 0.95f, 1.02f, 0.25f), new(0.25f, 0.15f, 0.28f), dark);
            Box("prag lateral", new(side * 0.97f, 0.36f, 0), new(0.14f, 0.16f, 2.25f), red);
            Box("suport eleron", new(side * 0.64f, 1.01f, -1.5f), new(0.1f, 0.45f, 0.12f), dark);
            Box("bandă capotă", new(side * 0.21f, 0.955f, 1.02f), new(0.16f, 0.025f, 0.8f), dark);
            foreach (float z in new[] { -1.18f, 1.18f })
            {
                SceneObject steering = AddObject("pivot direcție", null, car);
                steering.Transform.Position = new Vector3(side * 0.94f, 0.38f, z);
                SceneObject rolling = AddObject("pivot rulare", null, steering);
                _carWheels.Add((steering, rolling, z > 0));
                SceneObject wheel = AddObject("anvelopă", tire, rolling);
                wheel.Tint = dark;
                wheel.Transform.Rotation.Z = 90;
                SceneObject hub = AddObject("jantă aliaj", rim, rolling);
                hub.Tint = silver;
                hub.Transform.Position.X = side * 0.03f;
                hub.Transform.Rotation.Z = 90;
                // Spițe contrastante: fac vizibilă rotația roții.
                for (int i = 0; i < 5; i++)
                {
                    SceneObject spoke = AddObject("spiță", cube, rolling);
                    spoke.Transform.Position.X = side * 0.18f;
                    spoke.Transform.Scale = new Vector3(0.025f, 0.44f, 0.045f);
                    spoke.Transform.Rotation.X = i * 36;
                    spoke.Tint = dark;
                }
            }
        }
        Box("eleron", new(0, 1.25f, -1.5f), new(2.12f, 0.09f, 0.4f), dark);
        return car;
    }

    private void UpdateCarWheels(float dt)
    {
        _wheelAngle = (_wheelAngle + MathHelper.RadiansToDegrees(_player.DriveSpeed * dt / 0.38f)) % 360;
        foreach (var wheel in _carWheels)
        {
            wheel.Steering.Transform.Rotation.Y = wheel.Front ? _player.SteeringAngle : 0;
            wheel.Rolling.Transform.Rotation.X = _wheelAngle;
        }
    }
}
