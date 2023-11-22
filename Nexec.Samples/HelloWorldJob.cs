using Nexec.Attributes;

namespace Nexec.Samples;

[Job]
public class HelloWorldJob
{
    public void Execute()
    {
        Console.WriteLine("Hello World!");
    }
}
