using System.Collections.Generic;
using Orleans;
using Turbo.Primitives.Networking;
using Turbo.Primitives.Orleans.Snapshots.Perk;

namespace Turbo.Primitives.Messages.Outgoing.Perk;

[GenerateSerializer, Immutable]
public sealed record PerkAllowancesMessageComposer : IComposer
{
    [Id(0)]
    public required List<PerkAllowanceSnapshot> Perks { get; init; }
}
