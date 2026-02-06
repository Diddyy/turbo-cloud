using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turbo.Messages.Registry;
using Turbo.Primitives.Messages.Incoming.Users;
using Turbo.Primitives.Messages.Outgoing.Notifications;

namespace Turbo.PacketHandlers.Users;

public class GetMOTDMessageHandler : IMessageHandler<GetMOTDMessage>
{
    public async ValueTask HandleAsync(
        GetMOTDMessage message,
        MessageContext ctx,
        CancellationToken ct
    )
    {
        await ctx.SendComposerAsync(
                new MOTDNotificationEventMessageComposer { Messages = new List<string>() },
                ct
            )
            .ConfigureAwait(false);
    }
}
