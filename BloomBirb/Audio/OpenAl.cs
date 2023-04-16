using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public static unsafe class OpenAl
{
    private static AL? al;

    public static AL Al
    {
        get
        {
            ArgumentNullException.ThrowIfNull(al);
            return al;
        }
    }

    private static ALContext alContext = null!;

    private static Device* device = null;
    private static Context* context = null;

    private static bool initialized;

    public static void CreateContext()
    {
        if (initialized)
        {
            Console.WriteLine("WARNING: Attempted to create another OpenAL context. Ignoring.");
            return;
        }

        alContext = ALContext.GetApi();
        al ??= AL.GetApi();
        device = alContext.OpenDevice("");
        context = alContext.CreateContext(device, null);

        alContext.MakeContextCurrent(context);
        initialized = true;
    }
}
