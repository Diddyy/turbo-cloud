using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.Extensions.Logging;
using Turbo.Primitives.Networking.Revisions;
using Turbo.Primitives.Packets;

namespace Turbo.Networking.Revisions;

public sealed class RevisionManager(ILogger<RevisionManager> logger) : IRevisionManager
{
    private readonly ILogger<RevisionManager> _logger = logger;

    private ImmutableDictionary<string, RevisionSnapshot> _snapshots =
        ImmutableDictionary<string, RevisionSnapshot>.Empty.WithComparers(
            StringComparer.OrdinalIgnoreCase
        );

    private readonly ConcurrentDictionary<string, IRevision> _rawRevisions = new(
        StringComparer.OrdinalIgnoreCase
    );

    public IDictionary<string, IRevision> Revisions => _rawRevisions;

    public IRevision? GetRevision(string revisionId) =>
        _rawRevisions.TryGetValue(revisionId, out var revision) ? revision : null;

    public void RegisterRevision(IRevision revision)
    {
        if (revision is null)
            return;

        _rawRevisions[revision.Revision] = revision;

        var snapshot = new RevisionSnapshot(
            revision.Revision,
            revision.Parsers.ToImmutableDictionary(kv => kv.Key, kv => kv.Value),
            revision.Serializers.ToImmutableDictionary(kv => kv.Key, kv => kv.Value)
        );

        ImmutableDictionary<string, RevisionSnapshot> current;
        ImmutableDictionary<string, RevisionSnapshot> updated;

        do
        {
            current = Volatile.Read(ref _snapshots);
            updated = current.SetItem(revision.Revision, snapshot);
        } while (
            !ReferenceEquals(
                Interlocked.CompareExchange(ref _snapshots, updated, current),
                current
            )
        );

        _logger.LogInformation("Revision Registered: {Revision}", revision.Revision);
    }

    public bool TryGetParser(string revisionName, int header, out IParser? parser)
    {
        var snapshots = Volatile.Read(ref _snapshots);

        if (snapshots.TryGetValue(revisionName, out var snapshot))
            return snapshot.Parsers.TryGetValue(header, out parser);

        parser = null;
        return false;
    }

    public bool TryGetSerializer(string revisionName, Type composerType, out ISerializer? serializer)
    {
        var snapshots = Volatile.Read(ref _snapshots);

        if (snapshots.TryGetValue(revisionName, out var snapshot))
            return snapshot.Serializers.TryGetValue(composerType, out serializer);

        serializer = null;
        return false;
    }

    private sealed record RevisionSnapshot(
        string Revision,
        ImmutableDictionary<int, IParser> Parsers,
        ImmutableDictionary<Type, ISerializer> Serializers
    );
}
