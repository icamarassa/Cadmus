namespace Cadmus.Collector.State;

public sealed class CollectorCheckpoint
{
    public long LastRecordId { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}