// dumitru.fagateanu@student.usv.ro
using Egc;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

if (OperatingSystem.IsMacOS())
{
    AssemblyLoadContext.Default.ResolvingUnmanagedDll += (assembly, name) =>
    {
        if (assembly != typeof(OpenTK.Windowing.GraphicsLibraryFramework.GLFW).Assembly ||
            !name.Contains("glfw", StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        string prefix = Environment.GetEnvironmentVariable("HOMEBREW_PREFIX")
            ?? (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "/opt/homebrew" : "/usr/local");
        string path = Path.Combine(prefix, "opt", "glfw", "lib", "libglfw.3.dylib");
        if (!File.Exists(path))
        {
            throw new DllNotFoundException($"GLFW lipsește: {path}. Rulați brew install glfw; pentru un prefix personalizat setați HOMEBREW_PREFIX.");
        }
        return NativeLibrary.Load(path);
    };
}

// Program.cs conține DOAR inițializarea aplicației:
// creăm fereastra (clasa Game) și pornim bucla principală.
using var game = new Game(800, 600, "EGC – Laborator 04");
game.Run();
