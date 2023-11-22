using Nexec.Attributes;

namespace Nexec.Samples;

[Job]
public class CalculationJob
{
    [Input]
    public int A { get; set; }

    [Input]
    public int B { get; set; }

    [Input]
    public string Op { get; set; } = "+";

    [Output]
    public int Result { get; set; }

    public void Execute()
    {
        switch (Op)
        {
            case "+":
                Console.WriteLine($"{A} + {B}");
                Result = A + B;
                break;
            case "-":
                Console.WriteLine($"{A} - {B}");
                Result = A - B;
                break;
            case "*":
                Console.WriteLine($"{A} * {B}");
                Result = A * B;
                break;
            case "/":
                Console.WriteLine($"{A} / {B}");
                Result = A / B;
                break;
            case "%":
                Console.WriteLine($"{A} % {B}");
                Result = A % B;
                break;
            default:
                Console.Error.WriteLine($"Operation '{Op}' not supported");
                return;
        }
    }
}