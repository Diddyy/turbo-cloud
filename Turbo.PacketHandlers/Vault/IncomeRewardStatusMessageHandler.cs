using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turbo.Messages.Registry;
using Turbo.Primitives.Messages.Incoming.Vault;
using Turbo.Primitives.Messages.Outgoing.Vault;
using Turbo.Primitives.Orleans.Snapshots.Vault;

namespace Turbo.PacketHandlers.Vault;

public class IncomeRewardStatusMessageHandler : IMessageHandler<IncomeRewardStatusMessage>
{
    public async ValueTask HandleAsync(
        IncomeRewardStatusMessage message,
        MessageContext ctx,
        CancellationToken ct
    )
    {
        await ctx.SendComposerAsync(
                new IncomeRewardStatusMessageComposer { IncomeRewards = new List<IncomeRewardSnapshot>() },
                ct
            )
            .ConfigureAwait(false);
    }
}
