namespace TPMS.Edge.Persistence;

public sealed class EdgeOptions
{
    public const string SectionName = "Edge";

    public Guid EdgeNodeId { get; set; }

    public Guid ParkingLotId { get; set; }

    public string CloudBaseUrl { get; set; } = "http://localhost:5080";

    public bool EnableMockLpr { get; set; } = true;
}
