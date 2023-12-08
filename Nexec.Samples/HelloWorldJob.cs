using Microsoft.Extensions.Logging;
using Nexec.Attributes;

namespace Nexec.Samples;

[Job]
public class HelloWorldJob
{
    private readonly ILogger<HelloWorldJob> _logger;

    public HelloWorldJob(ILogger<HelloWorldJob> logger)
    {
        _logger = logger;
    }

    public void Execute()
    {
        _logger.LogInformation("Hello World!");
    }
}
