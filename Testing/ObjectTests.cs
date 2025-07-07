using NUnit.Framework;
using OpenGL;
using OpenTK.Mathematics;
using Object = OpenGL.Object;

[TestFixture]
public class ObjectTests
{
    private class TestObject : Object
    {
        private static readonly Vector3 DefaultPosition = Vector3.Zero;
        private static readonly Vector3 DefaultRotation = Vector3.Zero;
        private static readonly Vector3 DefaultScale = Vector3.One;
        private static readonly System.Numerics.Vector3 DefaultColor = new(1, 1, 1);
        private const float DefaultMass = 1.0f;
        private const string DefaultName = "Test";
        private const string DefaultType = "TestType";

        public TestObject() : base(
            name: DefaultName,
            position: DefaultPosition,
            rotation: DefaultRotation,
            scale: DefaultScale,
            color: DefaultColor,
            mass: DefaultMass,
            type: DefaultType)
        {
        }

        // Add public accessor for testing
        public Vector3 Velocity
        {
            get => base.Velocity;
            set => base.Velocity = value;
        }
    }

    [Test]
    public void GetModelMatrix_CombinesTransformationsCorrectly()
    {
        // Arrange
        var obj = new TestObject
        {
            Position = new Vector3(2, 0, 0),
            Rotation = new Vector3(0, 90, 0),
            Scale = new Vector3(2, 2, 2)
        };

        // Act
        var modelMatrix = obj.GetModelMatrix();

        // Assert
        Assert.Multiple(() =>
        {
            // Check translation component
            Vector3 expectedTranslation = new Vector3(2, 0, 0);
            Assert.That(modelMatrix.Row3.Xyz, Is.EqualTo(expectedTranslation).Using<Vector3>((a, b) => 
                (a - b).Length < 0.001f));

            // Check rotation component (90° around Y-axis means x-axis should be transformed to z-axis)
            Assert.That(modelMatrix.M11, Is.EqualTo(0f).Within(0.001f), "X-axis rotation incorrect");
            Assert.That(modelMatrix.M13, Is.EqualTo(2f).Within(0.001f), "Z-axis rotation incorrect");
        });
    }

    [Test]
    public void Update_IntegratesVelocityAndAcceleration()
    {
        // Arrange
        var obj = new TestObject
        {
            Velocity = new Vector3(1, 0, 0),
            Acceleration = new Vector3(0.5f, 0, 0)
        };
        float deltaTime = 1.0f;

        // Act
        obj.Update(deltaTime);

        // Assert
        Assert.Multiple(() =>
        {
            Vector3 expectedVelocity = new Vector3(1.5f, 0, 0);
            Vector3 expectedPosition = new Vector3(1.5f, 0, 0);

            Assert.That(obj.Velocity, Is.EqualTo(expectedVelocity).Using<Vector3>((a, b) => 
                (a - b).Length < 0.001f), "Velocity was not updated correctly");
            Assert.That(obj.Position, Is.EqualTo(expectedPosition).Using<Vector3>((a, b) => 
                (a - b).Length < 0.001f), "Position was not updated correctly");
        });
    }
}