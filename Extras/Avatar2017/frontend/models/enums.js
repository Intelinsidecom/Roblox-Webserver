
export const ModerationStatus = {
    ReviewApproved: 1,
    AwaitingApproval: 2,
    Declined: 3,
    AwaitingModerationDecision: 4
}

export const ModerationStatusStr = {
    ReviewApproved: "ReviewApproved",
    AwaitingApproval: "AwaitingApproval",
    Declined: "Declined",
    AwaitingModerationDecision: "AwaitingModerationDecision"
}

export const CreatorType = Object.freeze({
    User: 1,
    Group: 2
});

export const CurrencyType = Object.freeze({
    Robux: 1,
    Tickets: 2
});

export const AssetType = Object.freeze({
    Image: 1,
    TeeShirt: 2,
    TShirt: 2, // alias
    Audio: 3,
    Mesh: 4,
    Lua: 5,
    Hat: 8,
    Place: 9,
    Model: 10,
    Shirt: 11,
    Pants: 12,
    Decal: 13,
    Head: 17,
    Face: 18,
    Gear: 19,
    Badge: 21,
    Animation: 24,
    Torso: 27,
    RightArm: 28,
    LeftArm: 29,
    LeftLeg: 30,
    RightLeg: 31,
    Package: 32,
    GamePass: 34,
    Plugin: 38,
    SolidModel: 39,
    MeshPart: 40,
    HairAccessory: 41,
    FaceAccessory: 42,
    NeckAccessory: 43,
    ShoulderAccessory: 44,
    FrontAccessory: 45,
    BackAccessory: 46,
    WaistAccessory: 47,
    ClimbAnimation: 48,
    DeathAnimation: 49,
    FallAnimation: 50,
    IdleAnimation: 51,
    JumpAnimation: 52,
    RunAnimation: 53,
    SwimAnimation: 54,
    WalkAnimation: 55,
    PoseAnimation: 56,
    EmoteAnimation: 61,
    Video: 62,
    Special: 500
});

export const Genre = Object.freeze({
    All: 0,
    Building: 13,
    Horror: 5,
    TownAndCity: 1,
    Military: 11,
    Comedy: 9,
    Medieval: 2,
    Adventure: 7,
    SciFi: 3,
    Naval: 6,
    FPS: 14,
    RPG: 15,
    Sports: 8,
    Fighting: 4,
    Western: 10,
    Skatepark: 18
});

export const UserAdvertisementTargetType = Object.freeze({
    Asset: 1,
    Group: 2
});

export const UserAdvertisementType = Object.freeze({
    Banner728x90: 1,
    SkyScraper160x600: 2,
    Rectangle300x250: 3
});

export const AssetVoteType = Object.freeze({
    Upvote: 1,
    Downvote: 2
});

export const CurrencySize = Object.freeze({
    "28x28": 1,
    "20x20": 2,
    "16x16": 3,
});

/**
 * @template {Record<string|number, V>} O
 * @template V
 * @param {O} frozen
 * @param {V} value
 * @returns {(keyof O)|null}
 * @constructor
 */
export function GetEnum(frozen, value) {
    for (const key in frozen) {
        if (frozen[key] === value) return key;
    }
    return null;
}
