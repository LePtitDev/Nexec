using Microsoft.Extensions.Logging;
using Nexec.Attributes;

namespace Nexec.Samples;

[Job]
public class CalculationJob
{
    private readonly ILogger<CalculationJob> _logger;

    public CalculationJob(ILogger<CalculationJob> logger)
    {
        _logger = logger;
    }

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
                _logger.LogInformation($"{A} + {B}");
                Result = A + B;
                break;
            case "-":
                _logger.LogInformation($"{A} - {B}");
                Result = A - B;
                break;
            case "*":
                _logger.LogInformation($"{A} * {B}");
                Result = A * B;
                break;
            case "/":
                _logger.LogInformation($"{A} / {B}");
                Result = A / B;
                break;
            case "%":
                _logger.LogInformation($"{A} % {B}");
                Result = A % B;
                break;
            default:
                _logger.LogError($"Operation '{Op}' not supported");
                return;
        }
    }
}