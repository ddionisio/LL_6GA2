
[System.Flags]
public enum FaceFlags {
    None = 0x0,

    Front = 0x1,
    Back = 0x2,
    Top = 0x4,
    Bottom = 0x8,
    Left = 0x10,
    Right = 0x20,

    All = Front | Back | Top | Bottom | Left | Right
}
