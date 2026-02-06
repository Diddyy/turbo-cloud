using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turbo.Messages.Registry;
using Turbo.Primitives.Messages.Incoming.Inventory;
using Turbo.Primitives.Messages.Outgoing.Inventory.Purse;
using Turbo.Primitives.Messages.Outgoing.Notifications;

namespace Turbo.PacketHandlers.Inventory.Purse;

public class GetCreditsInfoMessageHandler : IMessageHandler<GetCreditsInfoMessage>
{
    public async ValueTask HandleAsync(
        GetCreditsInfoMessage message,
        MessageContext ctx,
        CancellationToken ct
    )
    {
        await ctx.SendComposerAsync(new CreditBalanceEventMessageComposer { Balance = "1337.0" }, ct)
            .ConfigureAwait(false);
        await ctx.SendComposerAsync(
                new ActivityPointsMessageComposer
                {
                    // Keep common activity-point categories present to match client startup expectations.
                    PointsByCategoryId = ImmutableDictionary.CreateRange(
                        new[]
                        {
                            new KeyValuePair<int, int>(0, 0),
                            new KeyValuePair<int, int>(1, 0),
                            new KeyValuePair<int, int>(2, 0),
                            new KeyValuePair<int, int>(3, 0),
                            new KeyValuePair<int, int>(4, 0),
                            new KeyValuePair<int, int>(5, 0),
                            new KeyValuePair<int, int>(6, 0),
                            new KeyValuePair<int, int>(7, 0),
                            new KeyValuePair<int, int>(101, 0),
                            new KeyValuePair<int, int>(102, 0),
                            new KeyValuePair<int, int>(103, 0),
                            new KeyValuePair<int, int>(104, 0),
                            new KeyValuePair<int, int>(105, 0),
                        }
                    ),
                },
                ct
            )
            .ConfigureAwait(false);
    }
}
