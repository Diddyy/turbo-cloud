using System.Threading;
using System.Threading.Tasks;
using Turbo.Messages.Registry;
using Turbo.Primitives.Messages.Incoming.Groupforums;
using Turbo.Primitives.Messages.Outgoing.Groupforums;

namespace Turbo.PacketHandlers.Groupforums;

public class GetUnreadForumsCountMessageHandler : IMessageHandler<GetUnreadForumsCountMessage>
{
    public async ValueTask HandleAsync(
        GetUnreadForumsCountMessage message,
        MessageContext ctx,
        CancellationToken ct
    )
    {
        await ctx.SendComposerAsync(
                new UnreadForumsCountMessageComposer { UnreadForumsCount = 0 },
                ct
            )
            .ConfigureAwait(false);
    }
}
