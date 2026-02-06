using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turbo.Messages.Registry;
using Turbo.Primitives.Messages.Incoming.Inventory.Badges;
using Turbo.Primitives.Messages.Outgoing.Inventory.Badges;

namespace Turbo.PacketHandlers.Inventory.Badges;

public class GetBadgePointLimitsMessageHandler : IMessageHandler<GetBadgePointLimitsMessage>
{
    public async ValueTask HandleAsync(
        GetBadgePointLimitsMessage message,
        MessageContext ctx,
        CancellationToken ct
    )
    {
        await ctx.SendComposerAsync(
                new BadgePointLimitsEventMessageComposer
                {
                    LimitsByBadgeCodePrefix = new List<BadgePointLimitGroup>(),
                },
                ct
            )
            .ConfigureAwait(false);
    }
}
