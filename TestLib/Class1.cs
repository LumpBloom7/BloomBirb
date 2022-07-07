namespace TestLib
{
    public class TestClass
    {
        public void SayHello()
        {
            while (true)
            {
                Console.WriteLine("Hello, Worldy!");
                Thread.Sleep(500);
            }
        }
    }
}
