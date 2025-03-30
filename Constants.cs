using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;

#pragma warning disable IDE1006 // Simplify object initialization
#pragma warning disable CA1707 // Remove unused private members
namespace OpenGL;

public static class Constants
{
    /// <summary>
    ///     Gravitational constant for simulation (6.67430 × 10^-11 in real world)
    /// </summary>
    public const float GRAVITATIONAL_CONSTANT = 0.1f; // Adjusted for simulation scale

    /// <summary>
    ///     Window Constants
    /// </summary>
    public static readonly int SCREEN_WIDTH = 800;

    public static readonly int SCREEN_HEIGHT = 600;
    public static readonly Vector3 INITIAL_CAMERA_POS = (0, 0, 5);


    /// <summary>
    ///     Sphere Data
    /// </summary>
    public static readonly float INITIAL_SPHERE_RADIUS = .3f;

    public static readonly float INITIAL_SPHERE_VELOCITY = .4f;
    public static readonly float DEFAULT_ORBIT_RADIUS = .2f;
    public static readonly int SPHERE_STACK_COUNT = 18;
    public static readonly int SPHERE_SECTOR_COUNT = 36;
    public static readonly float DEFAULT_SPHERE_MASS = 1.0f;


    /// <summary>
    ///     Skybox values
    /// </summary>
    // Skybox texture paths (order: right, left, top, bottom, front, back)
    public static readonly string[] SkyboxTexturePaths = new[]
    {
        "./ImageDependencies/Skybox/right.png",
        "./ImageDependencies/Skybox/left.png",
        "./ImageDependencies/Skybox/top.png",
        "./ImageDependencies/Skybox/bottom.png",
        "./ImageDependencies/Skybox/front.png",
        "./ImageDependencies/Skybox/back.png"
    };

    public static readonly string skyboxVertexShaderPath = "Shaders/Skybox.vert";
    public static readonly string skyboxFragmentShaderPath = "Shaders/Skybox.frag";

    /// <summary>
    ///     Basic values depending on planet type
    /// </summary>
    public static readonly float STAR_MASS = 100.0f;

    public static readonly float GAS_GIANT_MASS = 15.0f;
    public static readonly float ROCKY_PLANET_MASS = 1.2f;
    public static readonly float MOON_MASS = 0.1f;
    public static readonly float DESERT_MASS = 0.8f;
    public static readonly float ICE_GIANT_MASS = 20f;

    /// <summary>
    ///     Colors based on the celestial type
    /// </summary>
    public static readonly System.Numerics.Vector3 ROCKY_PLANET_COLOR = new(0.4f, 0.3f, 0.2f); // Earth-like brown

    public static readonly System.Numerics.Vector3 STAR_COLOR = new(1.0f, 0.9f, 0.7f); // Yellow-white star
    public static readonly System.Numerics.Vector3 GAS_GIANT_COLOR = new(0.8f, 0.6f, 0.4f); // Jupiter-like tan
    public static readonly System.Numerics.Vector3 MOON_COLOR = new(0.5f, 0.5f, 0.5f); // Gray moon
    public static readonly System.Numerics.Vector3 DESERT_PLANET_COLOR = new(0.7f, 0.5f, 0.3f); // Mars-like red
    public static readonly System.Numerics.Vector3 ICE_GIANT_COLOR = new(0.5f, 0.7f, 0.9f); // Neptune-like blue


    /// <summary>
    ///     Indicator values
    /// </summary>
    public static readonly string indicatorVertexShaderPath = "Shaders/Indicator.vert";

    public static readonly string indicatorFragmentShaderPath = "Shaders/Indicator.frag";
    public static readonly Vector3 INDICATOR_COLOR_DEF = new(0.0f, 1.0f, 0.0f);
    public static readonly float INDICATOR_ALPHA_DEF = 0.4f;

    //Centralized values which aren't constant
    public static Vector3 INDICATOR_COLOR = new(0.0f, 0.0f, 0.0f);
    public static float INDICATOR_ALPHA = 0.0f; // Semi-transparency

    /// <summary>
    ///     Rendered values
    /// </summary>
    public static readonly float PROJECTION_MATRIX_RADIAN_CONSTANT = 45f;

    public static readonly float NEAR_DEPTH_CONSTANT = .1f;
    public static readonly float FAR_DEPTH_CONSTANT = 500f;

    /// <summary>
    ///     Grid value(s)
    /// </summary>
    public static readonly int GRID_SIZE = 200;

    public static readonly float GRID_STEP = 1.0f;

    /// <summary>
    ///     Grid color values
    /// </summary>
    public static readonly float GRID_RED_VALUE = 1.0f;

    public static readonly float GRID_ALPHA_VALUE = .3f;
    public static readonly float GRID_FALLBACK_FLOAT = .5f;
    public static readonly float GRID_COMPARISON_FLOAT = .001f;
    public static readonly float GRID_YPOS_FLOAT = -1f;

    /// <summary>
    ///     Miscellaneous
    /// </summary>
    public static readonly float FLOAT_ZERO = 0.0f;


    /// <summary>
    ///     Camera values
    /// </summary>
    public static readonly float CAMERA_ANGLE_CLAMP = 89f;

    public static readonly float CAMERA_SPEED = 2.5f;
    public static readonly float CAMERA_SENSITIVITY = .2f;

    /// <summary>
    ///     Vertex and Index values
    /// </summary>
    public static readonly int VERTEX_ATRIBB_STRIDE = 7;

    public static readonly int VERTEX_ATRIBB_SIZE = 3;


    /// <summary>
    ///     Bespoke GUI values
    /// </summary>
    public static readonly Vector2 BESPOKE_WINDOW_SIZE = new(SCREEN_WIDTH, SCREEN_HEIGHT);

    public static readonly Vector2 BESPOKE_BUTTON_SIZE = new(50, 20);
    public static readonly uint BESPOKE_TEXTEDIT_WIDTH = 50;
    public static readonly uint BESPOKE_TEXTEDIT_WIDE_WIDTH = 120;

    public static readonly string DEFAULT_MASS_BUFFER = "1,2";
    public static readonly string DEFAULT_NAME_BUFFER = "Name me!";


    public static readonly string[] planetTypes = new[]
        { "Ocean planet", "Star", "Gas Giant", "Moon", "Desert planet", "Ice Giant" };

    /// <summary>
    ///     FilePaths
    /// </summary>
    public static readonly string vertexShaderPath = "Shaders/Shader.vert";

    public static readonly string fragmentShaderPath = "Shaders/Shader.frag";

    public static readonly string gridVertexShaderPath = "Shaders/Grid.vert";
    public static readonly string gridFragmentShaderPath = "Shaders/Grid.frag";

    public static readonly string LineVertPath = "Shaders/line.vert";
    public static readonly string LineFragPath = "Shaders/line.frag";


    /// <summary>
    ///     Calculate the approximate gravitational force between two simulated celestial bodies
    /// </summary>
    /// <param name="mass1"> Mass of the first celestial body</param>
    /// <param name="mass2"> Mass of the second celestial body</param>
    /// <param name="distance"> Distance between said objects</param>
    /// <returns></returns>
    public static float G_FORCE(float mass1, float mass2, float distance)
    {
        return mass1 * mass2 / float.Pow(distance, 2);
    }
}