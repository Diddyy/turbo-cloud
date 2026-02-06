using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turbo.Messages.Registry;
using Turbo.Primitives.Messages.Incoming.Users;
using Turbo.Primitives.Messages.Outgoing.Nft;

namespace Turbo.PacketHandlers.Users;

public class GetUserNftChatStylesMessageHandler : IMessageHandler<GetUserNftChatStylesMessage>
{
    public async ValueTask HandleAsync(
        GetUserNftChatStylesMessage message,
        MessageContext ctx,
        CancellationToken ct
    )
    {
        await ctx.SendComposerAsync(
                new UserNftChatStylesMessageComposer { ChatStyleIds = new List<int>() },
                ct
            )
            .ConfigureAwait(false);
    }
}
