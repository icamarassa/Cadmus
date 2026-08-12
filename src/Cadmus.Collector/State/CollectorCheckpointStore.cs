using System.Text.Json;

namespace Cadmus.Collector.State;

public sealed class CollectorCheckpointStore
{
    private static readonly string StateDirectory = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData),
        "Cadmus",
        "Collector");

    private static readonly string StateFilePath = Path.Combine(
        StateDirectory,
        "checkpoint.json");

    public async Task<long> GetLastRecordIdAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(StateFilePath))
        {
            return 0;
        }

        await using var stream = File.OpenRead(StateFilePath);

        var checkpoint = await JsonSerializer.DeserializeAsync<CollectorCheckpoint>(
            stream,
            cancellationToken: cancellationToken);

        return checkpoint?.LastRecordId ?? 0;
    }

    public async Task SaveLastRecordIdAsync(
        long recordId,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(StateDirectory);

        var checkpoint = new CollectorCheckpoint
        {
            LastRecordId = recordId,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(
            checkpoint,
            new JsonSerializerOptions { WriteIndented = true });

        var temporaryFilePath = $"{StateFilePath}.{Guid.NewGuid()}.tmp";

        await File.WriteAllTextAsync(
            temporaryFilePath,
            json,
            cancellationToken);

        File.Move(temporaryFilePath, StateFilePath, overwrite: true);
    }
}