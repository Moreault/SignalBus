namespace ToolBX.SignalBus;

internal record TriggeredSignal
{
    internal required object Identifier { get; init; }
    internal object? Arguments { get; init; }
}