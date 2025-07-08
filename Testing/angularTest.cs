using NUnit.Framework;
using OpenTK.Mathematics;

namespace OpenGL.Testing;

[TestFixture]
public class SphereTests
{
    [SetUp]
    public void Setup()
    {
        WindowManager.IsTestEnvironment = true;
        WindowManager.GlobalTime = 0; // Reset global time at start of each test

        _parentSphere = new Sphere(
            "Sun",
            Vector3.Zero,
            Vector3.Zero,
            new Vector3(2, 2, 2),
            new System.Numerics.Vector3(1, 1, 0), // Yellow
            0f, // No orbit for parent
            1.0f,
            "Star"
        );

        _childSphere = new Sphere(
            "Earth",
            new Vector3(10, 0, 0),
            Vector3.Zero,
            new Vector3(1, 1, 1),
            new System.Numerics.Vector3(0, 0, 1), // Blue
            10.0f, // Orbit radius
            0.5f, // Angular speed
            "Planet",
            _parentSphere
        );
    }

    [TearDown]
    public void TearDown()
    {
        WindowManager.IsTestEnvironment = false;
    }

    private Sphere _parentSphere;
    private Sphere _childSphere;

    [Test]
    public void ParentChild_Relationship_IsProperlyEstablished()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_childSphere.Parent, Is.EqualTo(_parentSphere));
            Assert.That(_parentSphere.Parent, Is.Null);
        });
    }

    [Test]
    [TestCase(0.0, 10, 0, 0)] // Initial position
    [TestCase(Math.PI / 2, 0, 0, 10)] // Quarter orbit
    [TestCase(Math.PI, -10, 0, 0)] // Half orbit
    public void Update_CalculatesCorrectOrbitPosition_ForChildSphere(double time, float expectedX, float expectedY,
        float expectedZ)
    {
        // Arrange
        WindowManager.GlobalTime = 0; // Reset time

        var parentSphere = new Sphere(
            "Parent",
            Vector3.Zero, // Parent at origin
            Vector3.Zero,
            Vector3.One,
            new System.Numerics.Vector3(1, 1, 1),
            0.0f, // No orbit for parent
            0.0f, // No angular speed for parent
            "ParentType"
        );

        var childSphere = new Sphere(
            "Child",
            new Vector3(10, 0, 0), // Start at x=10
            Vector3.Zero,
            Vector3.One,
            new System.Numerics.Vector3(1, 1, 1),
            10.0f, // 10 unit orbit radius
            1.0f, // Angular speed of 1
            "ChildType",
            parentSphere // Set the parent reference
        );

        // Set time and update
        WindowManager.GlobalTime = (float)time;
        parentSphere.Update(0.016f);
        childSphere.Update(0.016f);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(childSphere.Position.X, Is.EqualTo(expectedX).Within(0.01));
            Assert.That(childSphere.Position.Y, Is.EqualTo(expectedY).Within(0.01));
            Assert.That(childSphere.Position.Z, Is.EqualTo(expectedZ).Within(0.01));
        });
    }

    [Test]
    public void Update_ChildPosition_RelativeToParent()
    {
        // Arrange
        WindowManager.GlobalTime = 0;

        // Act
        _parentSphere.Update(0.016);
        _childSphere.Update(0.016);

        // Assert
        float expectedDistance = _childSphere.OrbitRadius;
        float actualDistance = (_childSphere.Position - _parentSphere.Position).Length;

        Assert.That(actualDistance, Is.EqualTo(expectedDistance).Within(0.01));
    }

    [Test]
    public void GenerateSphere_WithZeroRadius_UsesDefaultRadius()
    {
        // Arrange
        var sphere = new Sphere(
            "TestSphere",
            Vector3.Zero,
            Vector3.Zero,
            Vector3.Zero, // Zero scale should trigger default radius
            new System.Numerics.Vector3(1, 1, 1),
            0,
            0,
            "TestType"
        );

        // Act
        (float[] vertices, uint[] indices) = sphere.GenerateSphere();

        // Assert
        // Check that vertices are not at origin (0,0,0)
        bool allVerticesAtOrigin = true;
        for (int i = 0; i < vertices.Length; i += 5) // Stride of 5 (x,y,z,u,v)
            if (Math.Abs(vertices[i]) > 0.001f ||
                Math.Abs(vertices[i + 1]) > 0.001f ||
                Math.Abs(vertices[i + 2]) > 0.001f)
            {
                allVerticesAtOrigin = false;
                break;
            }

        Assert.That(allVerticesAtOrigin, Is.False, "Sphere should use default radius when scale is zero");
    }

    [Test]
    public void GetModelMatrix_IncludesAllTransformations()
    {
        // Arrange
        var position = new Vector3(1, 2, 3);
        var rotation = new Vector3(45, 90, 180);
        var scale = new Vector3(2, 2, 2);

        var sphere = new Sphere(
            "TestSphere",
            position,
            rotation,
            scale,
            new System.Numerics.Vector3(1, 1, 1),
            0,
            0,
            "TestType"
        );

        // Act
        var modelMatrix = sphere.GetModelMatrix();

        // Assert
        // Check translation (position is in the last column)
        Assert.Multiple(() =>
        {
            // Check position (translation is in the last column)
            Assert.That(modelMatrix.M41, Is.EqualTo(position.X).Within(0.01f));
            Assert.That(modelMatrix.M42, Is.EqualTo(position.Y).Within(0.01f));
            Assert.That(modelMatrix.M43, Is.EqualTo(position.Z).Within(0.01f));

            // Check scale (diagonal elements, considering rotation)
            float scaleX = new Vector3(modelMatrix.M11, modelMatrix.M12, modelMatrix.M13).Length;
            float scaleY = new Vector3(modelMatrix.M21, modelMatrix.M22, modelMatrix.M23).Length;
            float scaleZ = new Vector3(modelMatrix.M31, modelMatrix.M32, modelMatrix.M33).Length;

            Assert.That(scaleX, Is.EqualTo(scale.X).Within(0.01f));
            Assert.That(scaleY, Is.EqualTo(scale.Y).Within(0.01f));
            Assert.That(scaleZ, Is.EqualTo(scale.Z).Within(0.01f));
        });
    }


    [Test]
    public void GenerateSphere_TextureCoordinatesAreValid()
    {
        // Arrange
        var sphere = new Sphere(
            "TestSphere",
            Vector3.Zero,
            Vector3.Zero,
            Vector3.One,
            new System.Numerics.Vector3(1, 1, 1),
            0,
            0,
            "TestType"
        );

        // Act
        (float[] vertices, _) = sphere.GenerateSphere();

        // Assert
        for (int i = 3; i < vertices.Length; i += 5) // Starting at U coordinate, stride of 5
        {
            float u = vertices[i];
            float v = vertices[i + 1];

            Assert.Multiple(() =>
            {
                Assert.That(u, Is.GreaterThanOrEqualTo(0.0f).And.LessThanOrEqualTo(1.0f),
                    "U coordinate should be in range [0,1]");
                Assert.That(v, Is.GreaterThanOrEqualTo(0.0f).And.LessThanOrEqualTo(1.0f),
                    "V coordinate should be in range [0,1]");
            });
        }
    }
}