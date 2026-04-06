namespace TPMS.Infrastructure.Lpr;

public sealed class MockLprRecognitionService
{
    private static readonly string[] DemoPlates = ["ABC123", "PARK24", "EV900", "WILDLIFE7"];
    private readonly Random _random = new();

    public string RecognizePlate()
    {
        return DemoPlates[_random.Next(DemoPlates.Length)];
    }
}
