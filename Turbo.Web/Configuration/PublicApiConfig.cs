using System;
using Turbo.Primitives.Players;
using Turbo.Primitives.Settings;

namespace Turbo.Web.Configuration;

/// <summary>
/// The public API's room endpoints (<c>Turbo:Web:PublicApi</c>): a room, and the rooms a player
/// owns, as Habbo's OpenAPI document has them (<c>/api/public/rooms/{roomId}</c>,
/// <c>/api/public/users/{id}/rooms</c>), so a client library written for Habbo works with this
/// hotel's address.
/// </summary>
public sealed class PublicApiConfig
{
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// A room's thumbnail, with <c>{roomId}</c> for its id, such as the navigator's thumbnail
    /// address (Habbo's: <c>.../navigator-thumbnail/hhus/{roomId}.png</c>). Empty leaves
    /// <c>thumbnailUrl</c> out.
    /// </summary>
    [UrlSetting]
    public string RoomThumbnailUrl { get; init; } = "";

    /// <summary>
    /// A room's full-size picture, with <c>{roomId}</c> (Habbo's: <c>.../fullroom-photo/hhus/{roomId}.png</c>).
    /// Empty leaves <c>imageUrl</c> out.
    /// </summary>
    [UrlSetting]
    public string RoomImageUrl { get; init; } = "";

    /// <summary>
    /// The hotel part of a player's unique id (<c>&lt;prefix&gt;-&lt;32 hex&gt;</c>): Habbo.com's
    /// is <c>hhus</c> (official API answers, 2026-10-16).
    /// </summary>
    public string UniqueIdPrefix { get; init; } = "hhus";

    /// <summary>
    /// 32 hex digits (an AES-128 key) the 32 hex of a player's unique id are made with. Not a
    /// secret, it only keeps the ids from being counted through; changing it changes every
    /// player's unique id, so a client that kept one no longer finds the player.
    /// </summary>
    public string UniqueIdKey { get; init; } = "5f1e0c3b9a7d4e2f8c6b1a0d3e5f7a9c";

    public PlayerUniqueIds UniqueIds() => new(UniqueIdPrefix, Convert.FromHexString(UniqueIdKey));
}
