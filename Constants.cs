using System.Drawing;
using OpenTK.Mathematics;
#pragma warning disable IDE1006 // Simplify object initialization
namespace OpenGL;

public static class Constants
{
    
    
    /// <summary>
    /// Window Constants
    /// </summary>
    public static readonly int SCREEN_WIDTH = 800;
    public static readonly int SCREEN_HEIGHT = 600;
    public static readonly Vector3 INITIAL_CAMERA_POS = (-1, 3, 10);


    /// <summary>
    /// Sphere Data
    /// </summary>
    public static readonly float INITIAL_SPHERE_RADIUS = .3f;
    public static readonly float INITIAL_SPHERE_VELOCITY = .1f;
    public static readonly float DEFAULT_ORBIT_RADIUS = .2f;
    public static readonly int SPHERE_STACK_COUNT = 18;
    public static readonly int SPHERE_SECTOR_COUNT = 36;


    /// <summary>
    /// Rendered values
    /// </summary>
    public static readonly float PROJECTION_MATRIX_RADIAN_CONSTANT = 45f;
    public static readonly float NEAR_DEPTH_CONSTANT = .1f;
    public static readonly float FAR_DEPTH_CONSTANT = 100f;

    /// <summary>
    /// Grid value(s)
    /// </summary>
    public static readonly int GRID_SIZE = 200;
    public static readonly float GRID_STEP = 1.0f;

    /// <summary>
    /// Grid color values 
    /// </summary>
    public static readonly float GRID_RED_VALUE = 1.0f;
    //public static readonly float GRID_GREEN_VALUE = 0f;
    //public static readonly float GRID_BLUE_VALUE = 0f;
    public static readonly float GRID_ALPHA_VALUE = .3f;
    public static readonly float GRID_FALLBACK_FLOAT = .5f;
    public static readonly float GRID_COMPARISON_FLOAT = .001f;
    public static readonly float GRID_YPOS_FLOAT = -1f;

    /// <summary>
    /// Miscellaneous
    /// </summary>  
    public static readonly float FLOAT_ZERO = 0.0f;
    
    
    /// <summary>
    /// Camera values
    /// </summary>
    public static readonly float CAMERA_ANGLE_CLAMP = 89f;
    public static readonly float CAMERA_SPEED = 2.5f;
    public static readonly float CAMERA_SENSITIVITY = .2f;
    
    /// <summary>
    /// Vertex and Index values
    /// </summary>
    public static readonly int VERTEX_ATRIBB_STRIDE  = 7;
    public static readonly int VERTEX_ATRIBB_SIZE = 3;
    
    
    /// <summary>
    /// Bespoke GUI values
    /// </summary>
    public static readonly System.Numerics.Vector2 BESPOKE_WINDOW_SIZE = new System.Numerics.Vector2(SCREEN_WIDTH, SCREEN_HEIGHT); 
    public static readonly System.Numerics.Vector2 BESPOKE_BUTTON_SIZE = new System.Numerics.Vector2(50, 20);
    public static readonly uint BESPOKE_TEXTEDIT_WIDTH = 50;
    public static readonly uint BESPOKE_TEXTEDIT_WIDE_WIDTH = 120;
    
    public static readonly string BESPOKE_TEXT_DEFAULT = "0.0";
    
    
    public static readonly string[] planetTypes = new[] { "Ocean planet", "Star", "Gas Giant","Moon", "Desert planet","Ice Giant"};

    ///<summary>
    /// Physika
    /// </summary>
    
    
    /// <summary>
    /// Calculate the approximate gravitational force between two simulated celestial bodies
    /// </summary>
    /// <param name="mass1"> Mass of the first celestial body</param>
    /// <param name="mass2"> Mass of the second celestial body</param>
    /// <param name="distance"> Distance between said objects</param>
    /// <returns></returns>
    public static float G_FORCE(float mass1, float mass2, float distance) { return (mass1*mass2) / (float.Pow(distance, 2)); }
    
    /// <summary>
    /// FilePaths
    /// </summary>
    public static readonly string vertexShaderPath = "Shaders/Shader.vert";
    public static readonly string fragmentShaderPath = "Shaders/Shader.frag";
    
    public static readonly string gridVertexShaderPath = "Shaders/Grid.vert";
    public static readonly string gridFragmentShaderPath = "Shaders/Grid.frag";
    




}