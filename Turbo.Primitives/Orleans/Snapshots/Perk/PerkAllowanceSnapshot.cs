using Orleans;

namespace Turbo.Primitives.Orleans.Snapshots.Perk;

[GenerateSerializer, Immutable]
public sealed record PerkAllowanceSnapshot
{
    [Id(0)]
    public required string Code { get; init; }

    [Id(1)]
    public required string ErrorMessage { get; init; }

    [Id(2)]
    public required bool IsAllowed { get; init; }
}
