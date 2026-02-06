using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turbo.Messages.Registry;
using Turbo.Primitives.Messages.Incoming.Users;
using Turbo.Primitives.Messages.Outgoing.Users;

namespace Turbo.PacketHandlers.Users;

public class GetIgnoredUsersMessageHandler : IMessageHandler<GetIgnoredUsersMessage>
{
    public async ValueTask HandleAsync(
        GetIgnoredUsersMessage message,
        MessageContext ctx,
        CancellationToken ct
    )
    {
        await ctx.SendComposerAsync(
                new IgnoredUsersMessageComposer { IgnoredUserIds = new List<int>() },
                ct
            )
            .ConfigureAwait(false);
    }
}
