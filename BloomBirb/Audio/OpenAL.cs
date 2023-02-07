using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public unsafe class OpenAL
{
    private static ALContext alContext = null!;
    public static AL AL { get; private set; } = null!;

    private static Device* device = null;
    private static Context* context = null;

    public static void CreateContext()
    {
        alContext ??= ALContext.GetApi();
        AL ??= AL.GetApi();
        device = alContext.OpenDevice("");
        context = alContext.CreateContext(device, null);

        alContext.MakeContextCurrent(context);
    }
}
