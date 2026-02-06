using System;
using System.Collections.Generic;
using Turbo.Primitives.Packets;

namespace Turbo.Primitives.Networking.Revisions;

public interface IRevisionManager
{
    public IDictionary<string, IRevision> Revisions { get; }

    public IRevision? GetRevision(string revisionName);

    public void RegisterRevision(IRevision revision);

    public bool TryGetParser(string revisionName, int header, out IParser? parser);

    public bool TryGetSerializer(string revisionName, Type composerType, out ISerializer? serializer);
}
