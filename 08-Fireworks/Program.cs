﻿using System;
using System.Diagnostics;
using CommandLine;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Globalization;
using System.Text;
using Util;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace _08_Fireworks;

using Vector3 = Vector3D<float>;
using Matrix4 = Matrix4X4<float>;

#region OPTIONS
public class Options
{
  [Option('w', "width", Required = false, Default = 800, HelpText = "Window width in pixels.")]
  public int WindowWidth { get; set; } = 800;

  [Option('h', "height", Required = false, Default = 600, HelpText = "Window height in pixels.")]
  public int WindowHeight { get; set; } = 600;

  [Option('p', "particles", Required = false, Default = 10000, HelpText = "Maximum number of particles.")]
  public int Particles { get; set; } = 10000;

  [Option('t', "texture", Required = false, Default = ":check:", HelpText = "User-defined texture.")]
  public string TextureFile { get; set; } = ":check:";
}
#endregion

#region PARTICLE DEFINITIONS
abstract class Particle
{
  private protected Random rnd = new((int)DateTime.Now.Ticks);

  /// <summary>
  /// Global rotation axis (all particles rotate around the same axis).
  /// </summary>
  public static Vector3 AxisDirection;

  /// <summary>
  /// Create a new particle.
  /// </summary>
  public Particle(double now) => Reset(now);

  /// <summary>
  /// World space coordinates of the object's center.
  /// </summary>
  public Vector3 Position { get; protected set; }

  /// <summary>
  /// Current RGB color.
  /// </summary>
  public Vector3 Color { get; protected set; }

  /// <summary>
  /// Current point size in pixels.
  /// </summary>
  public float Size { get; protected set; }

  /// <summary>
  /// Rotation velocity in radians per second.
  /// </summary>
  public double Velocity { get; protected set; }

  /// <summary>
  /// Current age (time-to-death).
  /// </summary>
  public double Age { get; set; }

  /// <summary>
  /// Last simulated time in seconds.
  /// </summary>
  private protected double SimulatedTime;

  public virtual void Reset(double now)
  {
    double x, y;
    do
    {
      x = 0.5 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() +
                 rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 3.0);
      y = 0.5 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() +
                 rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 3.0);
    }
    while (x * x + y * y > 1.0);
    double z = 0.6 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 1.5);
    double r = Math.Sqrt(x * x + y * y);
    z *= 0.3 * Math.Exp(-4.0 * r * r);

    Position = new Vector3((float)x, (float)y, (float)z);

    Color = new Vector3(
      (float)(0.3 + 0.7 * rnd.NextDouble()),
      (float)(0.3 + 0.7 * rnd.NextDouble()),
      (float)(0.3 + 0.7 * rnd.NextDouble()));

    Size = 2.0f;

    Velocity = (1.2 - r) * (0.1 + 0.07 * rnd.NextDouble());

    Age = 6.0 + 4.0 * rnd.NextDouble();

    SimulatedTime = now;
  }

  /// <summary>
  /// Simulate one step in time.
  /// </summary>
  /// <param name="time">Target time to simulate to (in seconds).</param>
  /// <returns></returns>
  public virtual bool SimulateTo(double time)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    SimulatedTime = time;

    Age -= dt;
    if (Age <= 0.0)
      return false;

    // Change particle color.
    if (Age < 6.0)
      Color *= (float)Math.Pow(0.2, dt);

    // Change particle size.
    if (Age < 5.0)
      Size *= (float)Math.Pow(0.8, dt);

    return true;
  }

  public void FillBuffer (float[] buffer, ref int i)
  {
    // offset  0: Position
    buffer[i++] = Position.X;
    buffer[i++] = Position.Y;
    buffer[i++] = Position.Z;

    // offset  3: Color
    buffer[i++] = Color.X;
    buffer[i++] = Color.Y;
    buffer[i++] = Color.Z;

    // offset  6: Normal
    buffer[i++] = 0.0f;
    buffer[i++] = 1.0f;
    buffer[i++] = 0.0f;

    // offset  9: Txt coordinates
    buffer[i++] = 0.5f;
    buffer[i++] = 0.5f;

    // offset 11: Point size
    buffer[i++] = Size;
  }
}

class RocketParticle : Particle
{
  public RocketParticle (double now) : base(now)
  {
    Size = 4.0f; 
  }
  public override void Reset (double now)
  {
    double x, y;
    do
    {
      x = 0.5 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() +
                 rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 3.0);
      y = 0.5 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() +
                 rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 3.0);
    }
    while (x * x + y * y > 1.0);


    double r = Math.Sqrt(x * x + y * y);

    double z = 2.0 + -1.0 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() + 1.0);

    Position = new Vector3((float)x, (float)y, (float)z);

    Color = new Vector3(
      (float)(0.3 + 0.7 * rnd.NextDouble()),
      (float)(0.3 + 0.7 * rnd.NextDouble()),
      (float)(0.3 + 0.7 * rnd.NextDouble()));

    Size = 0.5f;

    // Set velocity to be directed upwards
    Velocity = (1.2 - r) * (5 + 3.5 * rnd.NextDouble()); 

    Age = 1.5;

    SimulatedTime = now;
  }
  public override bool SimulateTo(double time)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    SimulatedTime = time;

    // move the particle up

    var x = Position.X;
    var y = Position.Y;
    var z = (Position.Z + (0.005f)*Age);

    Position = new Vector3((float)x, (float)y, (float)z);

    // Decrease Age

    Age -= dt;
    if (Age <= 0.0)
      return false;

    return true;
  }
}

class ExplosionParticle : Particle
{
  private Vector3 Direction = new Vector3(0.2f, 0.2f, -0.5f);
  public ExplosionParticle (double now, Vector3 color, Vector3 direction, Vector3 position) : base(now) 
  {
    Size = 3.0f;
    Color = color;
    Position = position;
    Direction = direction;
  }

  public override void Reset (double now)
  {
    double x, y;
    do
    {
      x = 0.5 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() +
                 rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 3.0);
      y = 0.5 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() +
                 rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 3.0);
    }
    while (x * x + y * y > 1.0);
    double z = 0.6 * (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 1.5);
    double r = Math.Sqrt(x * x + y * y);
    z *= 0.3;
    Position = new Vector3((float)x, (float)y, (float)z);

    Size = 2.0f;

    // Set velocity to be directed upwards
    Velocity = (0.12 - r) * (0.01 + 0.007 * rnd.NextDouble());

    Age = 4.0 * rnd.NextDouble();

    SimulatedTime = now;
  }
  public override bool SimulateTo (double time)
  {
    if (time <= SimulatedTime)
      return true;

    double dt = time - SimulatedTime;
    SimulatedTime = time;

    // move the particle up

    var x = Position.X + Direction.X;
    var y = Position.Y + Direction.Y;
    var z = Position.Z + Direction.Z*-1;
    Position = new Vector3((float)x, (float)y, (float)z);

    // Decrease Age
    Age -= dt;
    if (Age <= 0.0)
      return false;

    // Change particle color.
    if (Age < 6.0)
      Color *= (float)Math.Pow(0.2, dt);

    // Change particle size.
    if (Age < 5.0)
      Size *= (float)Math.Pow(0.8, dt);

    return true;
  }
}
#endregion

#region SIMULATION
public class Simulation
{
  /// <summary>
  /// Dynamic array of all current particles.
  /// </summary>
  private List<Particle> particles = new();

  private protected Random rnd = new((int)DateTime.Now.Ticks);

  /// <summary>
  /// Actual number of particles.
  /// </summary>
  public int Particles => particles.Count;

  /// <summary>
  /// Particle number limit.
  /// </summary>
  public int MaxParticles { get; private set; }

  private int RocketParticles { get; set; }

  /// <summary>
  /// Last simulated time in seconds.
  /// </summary>
  private double SimulatedTime;

  /// <summary>
  /// Initialize a new particle simulator.
  /// </summary>
  /// <param name="now">Current real time in seconds.</param>
  /// <param name="particleRate">Maximum particle generation rate in particles per second.</param>
  /// <param name="maxParticles">Maximum number of particles in the system (can be slightly exceeded).</param>
  /// <param name="initParticles">Initial number of particles (the rest will be generated later).</param>
  public Simulation (double now, double particleRate, int maxParticles, int initParticles)
  {
    SimulatedTime = now;
    MaxParticles = maxParticles;
    // Generate(initParticles);
  }

  public void Generate()
  {
    int number = MaxParticles - particles.Count - RocketParticles*750;
    if (number <= 0)
      return;

    if ((number -= 750) > 0)
    {
      particles.Add(new RocketParticle(SimulatedTime));
      RocketParticles++;
    }
  }

  public void SimulateTo(double time)
  {
    if (time <= SimulatedTime)
      return;

    double dt = time - SimulatedTime;
    SimulatedTime = time;

    // 1. Simulate all the particles.
    List<int> toRemove = new();
    for (int i = 0; i < particles.Count; i++)
    {
      // Simulate one particle.
      if (!particles[i].SimulateTo(time))
      {
        // Delete the particle.
        toRemove.Add(i);
      }
    }

    // 2. Remove the dead ones.
    for (var i = toRemove.Count; --i >= 0;)
    {
      if (particles.ElementAt(toRemove[i]) is RocketParticle)
      {
        RocketParticles--;
        var color = particles.ElementAt(toRemove[i]).Color;
        int choice = rnd.Next(100); // Generate a random number between 0 and 99
        Vector3 position = particles.ElementAt(toRemove[i]).Position;

        if (choice < 60) // 0-59 (60% probability for Spherical)
        {
          SphereExplosion(ref particles, i, color, position);
        }
        else if (choice < 80) // 60-79 (20% probability for Cubic)
        {
          CubeExplosion(ref particles, i, color, position);
        }
        else // 80-100 (20% probability for Sparkles)
        {
          SparkleExplosion(ref particles, i, color, position);
        }
      }
      particles.RemoveAt(toRemove[i]);
    }

    // different explosion creations

  }

  // Explosion Types
  void SphereExplosion (ref List<Particle> particles, int i, Vector3 color, Vector3 position )
  {
    for (int j = 0; j < 750; j++)
    {
      //  Vector3 color = particles.ElementAt(i).Color;
      // Generate random directions with a starburst pattern
      float angle = (float)(2.0 * Math.PI * rnd.NextDouble()); // Random angle around the z-axis
      float inclination = (float)Math.Acos(2.0 * rnd.NextDouble() - 1.0); // Random inclination for full sphere

      // Convert spherical coordinates to Cartesian coordinates
      Vector3 direction = new Vector3(
                    (float)Math.Sin(inclination) * (float)Math.Cos(angle),
                    (float)Math.Sin(inclination) * (float)Math.Sin(angle),
                    (float)Math.Cos(inclination)
                );
      direction *= 0.005f;
      particles.Add(new ExplosionParticle(SimulatedTime, color, direction, position));
    }
  }
  /*
  void SplashExplosion (ref List<Particle> particles, int i, Vector3 color)
  {
    Vector3 cubeCenter = particles[i].Position; // Use the position of the current particle as the cube center
    float cubeSize = 0.5f; // Define the size of the cube (distance from center to any face)
    int particlesPerVertex = 750 / 8; // Define how many particles to generate per vertex

    // Loop over each vertex of the cube
    for (int x = -1; x <= 1; x += 2)
    {
      for (int y = -1; y <= 1; y += 2)
      {
        for (int z = -1; z <= 1; z += 2)
        {
          Vector3 vertexPosition = new Vector3(x * cubeSize / 2, y * cubeSize / 2, z * cubeSize / 2) + cubeCenter;
          for (int p = 0; p < particlesPerVertex; p++)
          {
            // Create a small random direction for each particle to give them a slight initial movement
            float angle = (float)(2.0 * Math.PI * rnd.NextDouble()); // Random angle
            float inclination = (float)(Math.PI * rnd.NextDouble()); // Random inclination for a semi-sphere
            Vector3 direction = new Vector3(
                        (float)Math.Sin(inclination) * (float)Math.Cos(angle),
                        (float)Math.Sin(inclination) * (float)Math.Sin(angle),
                        (float)Math.Cos(inclination)
                    ) * 0.005f; // Small magnitude for slight movement

            // Add the new particle at the vertex with a slight random direction
            particles.Add(new ExplosionParticle(SimulatedTime, color, direction, vertexPosition));
          }
        }
      }
    }
  }
  */
  void CubeExplosion (ref List<Particle> particles, int i, Vector3 color, Vector3 torusCenter)
  {
    float majorRadius = 0.005f; // Major radius (distance from the center of the torus tube to the center of the torus)
    float minorRadius = 0.0025f; // Minor radius (radius of the torus tube)
    int rings = 75; // Number of rings to fully represent the torus
    int particlesPerRing = 750 / rings; // Number of particles per ring

    // Loop to create multiple rings
    for (int ring = 0; ring < rings; ring++)
    {
      float theta = (float)(2 * Math.PI * ring / rings); // Angle for the major radius
      Vector3 ringCenter = new Vector3(
            torusCenter.X + majorRadius * (float)Math.Cos(theta),
            torusCenter.Y + majorRadius * (float)Math.Sin(theta),
            torusCenter.Z
        );

      // Generate particles around the ring
      for (int p = 0; p < particlesPerRing; p++)
      {
        float phi = (float)(2 * Math.PI * p / particlesPerRing); // Angle for the minor radius
        Vector3 particlePosition = new Vector3(
                ringCenter.X + minorRadius * (float)Math.Cos(phi) * (float)Math.Cos(theta),
                ringCenter.Y + minorRadius * (float)Math.Cos(phi) * (float)Math.Sin(theta),
                ringCenter.Z + minorRadius * (float)Math.Sin(phi)
            );

        Vector3 direction = new Vector3(
                (float)(rnd.NextDouble() - 0.5) * 0.01f,
                (float)(rnd.NextDouble() - 0.5) * 0.01f,
                (float)(rnd.NextDouble() - 0.5) * 0.01f
            );

        particles.Add(new ExplosionParticle(SimulatedTime, color, direction, particlePosition));
      }
    }
  }
  void SparkleExplosion (ref List<Particle> particles, int i, Vector3 color, Vector3 position)
  {
    int sparklesCount = 50; // Number of sparkles to generate

    for (int j = 0; j < sparklesCount; j++)
    {
      float angle = (float)(2.0 * Math.PI * rnd.NextDouble()); // Random angle
      float inclination = (float)(Math.PI * rnd.NextDouble()); // Full sphere inclination
      Vector3 direction = new Vector3(
            (float)Math.Sin(inclination) * (float)Math.Cos(angle),
            (float)Math.Sin(inclination) * (float)Math.Sin(angle),
            (float)Math.Cos(inclination)
        ) * 0.02f; // Higher velocity for sparkles

      particles.Add(new ExplosionParticle(SimulatedTime, color, direction, position));
    }
  }


  /// <summary>
  /// [Re]fills the vertex buffer with current data.
  /// </summary>
  /// <param name="buffer">Vertex buffer array.</param>
  /// <returns>Number of particles to update and render.</returns>
  public int FillBuffer (float[] buffer)
  {
    int i = 0;
    foreach (var p in particles)
      p.FillBuffer(buffer, ref i);

    return particles.Count;
  }

  /// <summary>
  /// Removes all the particles.
  /// </summary>
  public void Reset()
  {
    particles.Clear();
  }
}
#endregion

#region PROGRAM
internal class Program
{
  // System objects.
  private static IWindow? window;
  private static GL? Gl;

  // VB locking (too lousy?)
  private static object renderLock = new();

  // Window size.
  private static float width;
  private static float height;

  // Trackball.
  private static Trackball? tb;

  // FPS counter.
  private static FPS fps = new();

  // Scene dimensions.
  private static Vector3 sceneCenter = Vector3.Zero;
  private static float sceneDiameter = 1.5f;

  // Global 3D data buffer.
  private const int MAX_VERTICES = 65536;
  private const int VERTEX_SIZE = 12;     // x, y, z, R, G, B, Nx, Ny, Nz, s, t, size

  /// <summary>
  /// Current dynamic vertex buffer in .NET memory.
  /// Better idea is to store the buffer on GPU and update it every frame.
  /// </summary>
  private static float[] vertexBuffer = new float[MAX_VERTICES * VERTEX_SIZE];

  /// <summary>
  /// Current number of vertices to draw.
  /// </summary>
  private static int vertices = 0;

  public static int maxParticles = 0;
  public static double particleRate = 1000.0;

  private static BufferObject<float>? Vbo;
  private static VertexArrayObject<float>? Vao;

  // Texture.
  private static Util.Texture? texture;
  private static bool useTexture = false;
  private static string textureFile = ":check:";
  private const int TEX_SIZE = 128;

  // Lighting.
  private static bool usePhong = false;

  // Shader program.
  private static ShaderProgram? ShaderPrg;

  private static double nowSeconds = FPS.NowInSeconds;

  // Particle simulation system.
  private static Simulation? sim;

  //////////////////////////////////////////////////////
  // Application.

  private static string WindowTitle()
  {
    StringBuilder sb = new("08-Fireworks");

    if (sim != null)
    {
      sb.Append(string.Format(CultureInfo.InvariantCulture, " [{0} of {1}]", sim.Particles, sim.MaxParticles));
    }

    sb.Append(string.Format(CultureInfo.InvariantCulture, ", fps={0:f1}", fps.Fps));
    if (window != null &&
        window.VSync)
      sb.Append(" [VSync]");

    double pps = fps.Pps;
    if (pps > 0.0)
      if (pps < 5.0e5)
        sb.Append(string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}k", pps * 1.0e-3));
      else
        sb.Append(string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}m", pps * 1.0e-6));

    if (tb != null)
    {
      sb.Append(tb.UsePerspective ? ", perspective" : ", orthographic");
      sb.Append(string.Format(CultureInfo.InvariantCulture, ", zoom={0:f2}", tb.Zoom));
    }

    if (useTexture &&
        texture != null &&
        texture.IsValid())
      sb.Append($", txt={texture.name}");
    else
      sb.Append(", no texture");

    if (usePhong)
      sb.Append(", Phong shading");

    return sb.ToString();
  }

  private static void SetWindowTitle()
  {
    if (window != null)
      window.Title = WindowTitle();
  }

  private static void Main(string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(o.WindowWidth, o.WindowHeight);
        options.Title = WindowTitle();
        options.PreferredDepthBufferBits = 24;
        options.VSync = true;

        window = Window.Create(options);
        width  = o.WindowWidth;
        height = o.WindowHeight;

        window.Load    += OnLoad;
        window.Render  += OnRender;
        window.Closing += OnClose;
        window.Resize  += OnResize;

        textureFile = o.TextureFile;
        maxParticles = Math.Min(MAX_VERTICES, o.Particles);

        window.Run();
      });
  }

  private static void VaoPointers()
  {
    Debug.Assert(Vao != null);
    Vao.Bind();
    Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  0);
    Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  3);
    Vao.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  6);
    Vao.VertexAttributePointer(3, 2, VertexAttribPointerType.Float, VERTEX_SIZE,  9);
    Vao.VertexAttributePointer(4, 1, VertexAttribPointerType.Float, VERTEX_SIZE, 11);
  }

  private static void OnLoad()
  {
    Debug.Assert(window != null);

    // Initialize all the inputs (keyboard + mouse).
    IInputContext input = window.CreateInput();
    for (int i = 0; i < input.Keyboards.Count; i++)
    {
      input.Keyboards[i].KeyDown += KeyDown;
      input.Keyboards[i].KeyUp   += KeyUp;
    }
    for (int i = 0; i < input.Mice.Count; i++)
    {
      input.Mice[i].MouseDown   += MouseDown;
      input.Mice[i].MouseUp     += MouseUp;
      input.Mice[i].MouseMove   += MouseMove;
      input.Mice[i].DoubleClick += MouseDoubleClick;
      input.Mice[i].Scroll      += MouseScroll;
    }

    // OpenGL global reference (shortcut).
    Gl = GL.GetApi(window);

    //------------------------------------------------------
    // Render data.

    // Init the rendering data.
    lock (renderLock)
    {
      // Initialize the simulation object and fill the VB.
      Particle.AxisDirection = Vector3.UnitZ;
      sim = new Simulation(nowSeconds, particleRate, maxParticles, maxParticles / 10);
      vertices = sim.FillBuffer(vertexBuffer);

      // Vertex Array Object = Vertex buffer + Index buffer.
      Vbo = new BufferObject<float>(Gl, vertexBuffer, BufferTargetARB.ArrayBuffer);
      Vao = new VertexArrayObject<float>(Gl, Vbo);
      VaoPointers();

      // Initialize the shaders.
      ShaderPrg = new ShaderProgram(Gl, "vertex.glsl", "fragment.glsl");

      // Initialize the texture.
      if (textureFile.StartsWith(":"))
      {
        // Generated texture.
        texture = new(TEX_SIZE, TEX_SIZE, textureFile);
        texture.GenerateTexture(Gl);
      }
      else
      {
        texture = new(textureFile, textureFile);
        texture.OpenglTextureFromFile(Gl);
      }

      // Trackball.
      tb = new(sceneCenter, sceneDiameter);
    }

    // Main window.
    SetWindowTitle();
    SetupViewport();
  }

  /// <summary>
  /// Mouse horizontal scaling coefficient.
  /// One unit/pixel of mouse movement corresponds to this distance in world space.
  /// </summary>
  private static float mouseCx =  0.001f;

  /// <summary>
  /// Mouse vertical scaling coefficient.
  /// Vertical scaling is just negative value of horizontal one.
  /// </summary>
  private static float mouseCy = -0.001f;

  /// <summary>
  /// Does all necessary steps after window setup/resize.
  /// Assumes valid values in 'width' and 'height'.
  /// </summary>
  private static void SetupViewport()
  {
    // OpenGL viewport.
    Gl?.Viewport(0, 0, (uint)width, (uint)height);

    tb?.ViewportChange((int)width, (int)height, 0.05f, 20.0f);

    // The tight coordinate is used for mouse scaling.
    float minSize = Math.Min(width, height);
    mouseCx = sceneDiameter / minSize;
    // Vertical mouse scaling is just negative...
    mouseCy = -mouseCx;
  }

  /// <summary>
  /// Called after window resize.
  /// </summary>
  /// <param name="newSize">New window size in pixels.</param>
  private static void OnResize(Vector2D<int> newSize)
  {
    width  = newSize[0];
    height = newSize[1];
    SetupViewport();
  }

  /// <summary>
  /// Called every time the content of the window should be redrawn.
  /// </summary>
  /// <param name="obj"></param>
  private static unsafe void OnRender(double obj)
  {
    Debug.Assert(Gl != null);
    Debug.Assert(ShaderPrg != null);
    Debug.Assert(tb != null);

    Gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);

    lock (renderLock)
    {
      // Simulation the particle system.
      nowSeconds = FPS.NowInSeconds;
      if (sim != null)
      {
        sim.SimulateTo(nowSeconds);
        vertices = sim.FillBuffer(vertexBuffer);
      }

      // Rendering properties (set in every frame for clarity).
      Gl.Enable(GLEnum.DepthTest);
      Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
      Gl.Disable(GLEnum.CullFace);
      Gl.Enable(GLEnum.VertexProgramPointSize);

      // Draw the scene (set of Object-s).
      VaoPointers();
      ShaderPrg.Use();

      // Shared shader uniforms - matrices.
      ShaderPrg.TrySetUniform("view", tb.View);
      ShaderPrg.TrySetUniform("projection", tb.Projection);
      ShaderPrg.TrySetUniform("model", Matrix4.Identity);

      // Shared shader uniforms - Phong shading.
      ShaderPrg.TrySetUniform("lightColor", 1.0f, 1.0f, 1.0f);
      ShaderPrg.TrySetUniform("lightPosition", -8.0f, 8.0f, 8.0f);
      ShaderPrg.TrySetUniform("eyePosition", tb.Eye);
      ShaderPrg.TrySetUniform("Ka", 0.1f);
      ShaderPrg.TrySetUniform("Kd", 0.7f);
      ShaderPrg.TrySetUniform("Ks", 0.3f);
      ShaderPrg.TrySetUniform("shininess", 60.0f);
      ShaderPrg.TrySetUniform("usePhong", usePhong);

      // Shared shader uniforms - Texture.
      if (texture == null || !texture.IsValid())
        useTexture = false;
      ShaderPrg.TrySetUniform("useTexture", useTexture);
      ShaderPrg.TrySetUniform("tex", 0);
      if (useTexture)
        texture?.Bind(Gl);

      // Draw the particle system.
      vertices = (sim != null) ? sim.FillBuffer(vertexBuffer) : 0;

      if (Vbo != null &&
          vertices > 0)
      {
        Vbo.UpdateData(vertexBuffer, 0, vertices * VERTEX_SIZE);

        // Draw the batch of points.
        Gl.DrawArrays((GLEnum)PrimitiveType.Points, 0, (uint)vertices);

        // Update Pps.
        fps.AddPrimitives(vertices);
      }
    }

    // Cleanup.
    Gl.UseProgram(0);
    if (useTexture)
      Gl.BindTexture(TextureTarget.Texture2D, 0);

    // FPS.
    if (fps.AddFrames())
      SetWindowTitle();
  }

  /// <summary>
  /// Handler for window close event.
  /// </summary>
  private static void OnClose()
  {
    Vao?.Dispose();
    ShaderPrg?.Dispose();

    // Remember to dispose the textures.
    texture?.Dispose();
  }

  /// <summary>
  /// Shift counter (0 = no shift pressed).
  /// </summary>
  private static int shiftDown = 0;

  /// <summary>
  /// Ctrl counter (0 = no ctrl pressed).
  /// </summary>
  private static int ctrlDown = 0;

  /// <summary>
  /// Handler function for keyboard key up.
  /// </summary>
  /// <param name="arg1">Keyboard object.</param>
  /// <param name="arg2">Key identification.</param>
  /// <param name="arg3">Key scancode.</param>
  private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
  {
    if (tb != null &&
        tb.KeyDown(arg1, arg2, arg3))
    {
      SetWindowTitle();
      //return;
    }

    switch (arg2)
    {
      case Key.ShiftLeft:
      case Key.ShiftRight:
        shiftDown++;
        break;

      case Key.ControlLeft:
      case Key.ControlRight:
        ctrlDown++;
        break;

      case Key.T:
        // Toggle texture.
        useTexture = !useTexture;
        if (useTexture)
          Ut.Message($"Texture: {texture?.name}");
        else
          Ut.Message("Texturing off");
        SetWindowTitle();
        break;

      case Key.I:
        // Toggle Phong shading.
        usePhong = !usePhong;
         Ut.Message("Phong shading: " + (usePhong ? "on" : "off"));
        SetWindowTitle();
        break;

      case Key.P:
        // Perspective <-> orthographic.
        if (tb != null)
        {
          tb.UsePerspective = !tb.UsePerspective;
          SetWindowTitle();
        }
        break;

      case Key.C:
        // Reset view.
        if (tb != null)
        {
          tb.Reset();
          Ut.Message("Camera reset");
        }
        break;

      case Key.Space:
        sim.Generate();
        // Generate
        break;
      case Key.V:
        // Toggle VSync.
        if (window != null)
        {
          window.VSync = !window.VSync;
          if (window.VSync)
          {
            Ut.Message("VSync on");
            fps.Reset();
          }
          else
            Ut.Message("VSync off");
        }
        break;

      case Key.R:
        // Reset the simulator.
        if (sim != null)
        {
          sim.Reset();
          Ut.Message("Simulator reset");
        }
        break;

      case Key.Up:
        // Increase particle generation rate.
        if (sim != null)
        {
          SetWindowTitle();
        }
        break;

      case Key.Down:
        // Decrease particle generation rate.
        if (sim != null)
        {
          SetWindowTitle();
        }
        break;

      case Key.F1:
        // Help.
        Ut.Message("T           toggle texture", true);
        Ut.Message("I           toggle Phong shading", true);
        Ut.Message("P           toggle perspective", true);
        Ut.Message("V           toggle VSync", true);
        Ut.Message("C           camera reset", true);
        Ut.Message("Space       launch fireworks", true);
        Ut.Message("R           reset the simulation", true);
        Ut.Message("Up, Down    change particle generation rate", true);
        Ut.Message("F1          print help", true);
        Ut.Message("Esc         quit the program", true);
        Ut.Message("Mouse.left  Trackball rotation", true);
        Ut.Message("Mouse.wheel zoom in/out", true);
        break;

      case Key.Escape:
        // Close the application.
        window?.Close();
        break;
    }
  }

  /// <summary>
  /// Handler function for keyboard key up.
  /// </summary>
  /// <param name="arg1">Keyboard object.</param>
  /// <param name="arg2">Key identification.</param>
  /// <param name="arg3">Key scancode.</param>
  private static void KeyUp(IKeyboard arg1, Key arg2, int arg3)
  {
    if (tb != null &&
        tb.KeyUp(arg1, arg2, arg3))
      return;

    switch (arg2)
    {
      case Key.ShiftLeft:
      case Key.ShiftRight:
        shiftDown--;
        break;

      case Key.ControlLeft:
      case Key.ControlRight:
        ctrlDown--;
        break;
    }
  }

  /// <summary>
  /// Mouse dragging - current X coordinate in pixels.
  /// </summary>
  private static float currentX = 0.0f;

  /// <summary>
  /// Mouse dragging - current Y coordinate in pixels.
  /// </summary>
  private static float currentY = 0.0f;

  /// <summary>
  /// True if dragging mode is active.
  /// </summary>
  private static bool dragging = false;

  /// <summary>
  /// Handler function for mouse button down.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Button identification.</param>
  private static void MouseDown(IMouse mouse, MouseButton btn)
  {
    if (tb != null)
      tb.MouseDown(mouse, btn);

    if (btn == MouseButton.Right)
    {
      Ut.MessageInvariant($"Right button down: {mouse.Position}");

      // Start dragging.
      dragging = true;
      currentX = mouse.Position.X;
      currentY = mouse.Position.Y;
    }
  }

  /// <summary>
  /// Handler function for mouse button up.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Button identification.</param>
  private static void MouseUp(IMouse mouse, MouseButton btn)
  {
    if (tb != null)
      tb.MouseUp(mouse, btn);

    if (btn == MouseButton.Right)
    {
      Ut.MessageInvariant($"Right button up: {mouse.Position}");

      // Stop dragging.
      dragging = false;
    }
  }

  /// <summary>
  /// Handler function for mouse move.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="xy">New mouse position in pixels.</param>
  private static void MouseMove(IMouse mouse, System.Numerics.Vector2 xy)
  {
    if (tb != null)
      tb.MouseMove(mouse, xy);

    if (mouse.IsButtonPressed(MouseButton.Right))
    {
      Ut.MessageInvariant($"Mouse drag: {xy}");
    }

    // Object dragging.
    if (dragging)
    {
      float newX = mouse.Position.X;
      float newY = mouse.Position.Y;

      if (newX != currentX || newY != currentY)
      {
        //if (Objects.Count > 0)
        //{
        //  LastObject.Translate(new((newX - currentX) * mouseCx, (newY - currentY) * mouseCy, 0.0f));
        //}

        currentX = newX;
        currentY = newY;
      }
    }
  }

  /// <summary>
  /// Handler function for mouse button double click.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Button identification.</param>
  /// <param name="xy">Double click position in pixels.</param>
  private static void MouseDoubleClick(IMouse mouse, MouseButton btn, System.Numerics.Vector2 xy)
  {
    if (btn == MouseButton.Right)
    {
      Ut.Message("Closed by double-click.", true);
      window?.Close();
    }
  }

  /// <summary>
  /// Handler function for mouse wheel rotation.
  /// </summary>
  /// <param name="mouse">Mouse object.</param>
  /// <param name="btn">Mouse wheel object (Y coordinate is used here).</param>
  private static void MouseScroll(IMouse mouse, ScrollWheel wheel)
  {
    if (tb != null)
    {
      tb.MouseWheel(mouse, wheel);
      SetWindowTitle();
    }

    // wheel.Y is -1 or 1
    Ut.MessageInvariant($"Mouse scroll: {wheel.Y}");
  }
}
#endregion
