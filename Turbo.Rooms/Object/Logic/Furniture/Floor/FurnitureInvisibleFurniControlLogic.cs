using System.Threading;
using System.Threading.Tasks;
using Turbo.Primitives.Furniture.Providers;
using Turbo.Primitives.Rooms.Object.Furniture.Floor;
using Turbo.Primitives.Rooms.Object.Logic;

namespace Turbo.Rooms.Object.Logic.Furniture.Floor;

/// <summary>
/// The Invisible Furni Controller (<c>conf_invis_control</c>). While one in the room is switched
/// on, every client in it hides the furni layers tagged <c>invisible</c>: the marks that show
/// builders where invisible click tiles and the like stand (Flash
/// <c>RoomEngine.setInvisibleFurni</c>, the last flag of <c>ConfigurationItemStates</c>, and
/// <c>FurnitureVisualization</c>, which draws those layers at alpha 0). The Room Area Hider's
/// "make this furniture invisible" option names this controller
/// (<c>widget.areahide.options.invisibility.info</c>): such a hider is taken out of the room
/// while one is on (<c>RoomFurniModule.RefreshInvisibleFurniAsync</c>). Off is the default
/// state, so the marks show until someone switches it on.
/// </summary>
[RoomObjectLogic("invisible_furni_control")]
public class FurnitureInvisibleFurniControlLogic(
    IStuffDataFactory stuffDataFactory,
    IRoomFloorItemContext ctx
) : FurnitureConfigurationControlLogic(stuffDataFactory, ctx)
{
    public override async Task OnStateChangedAsync(CancellationToken ct)
    {
        await base.OnStateChangedAsync(ct);
        await FurniModule.RefreshInvisibleFurniAsync(null, ct);
    }

    public override async Task OnAttachAsync(CancellationToken ct)
    {
        await base.OnAttachAsync(ct);
        await FurniModule.RefreshInvisibleFurniAsync(null, ct);
    }

    public override async Task OnDetachAsync(CancellationToken ct)
    {
        await base.OnDetachAsync(ct);
        await FurniModule.RefreshInvisibleFurniAsync(_ctx.ObjectId, ct);
    }
}
