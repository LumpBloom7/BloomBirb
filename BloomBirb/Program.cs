using TestLib;

namespace BloomBirb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine(GetString());
                Thread.Sleep(500);
            }
        }

        public static string GetString() => "Hello World";
    }
}
