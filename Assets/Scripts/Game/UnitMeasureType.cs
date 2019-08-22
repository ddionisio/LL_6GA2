using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitMeasureType {
    Feet,
    Meter,
}

public struct UnitMeasure {
    public static string GetText(UnitMeasureType type) {
        var textRef = type.ToString().ToLower();
        return M8.Localize.Get(textRef);
    }

    private static System.Text.StringBuilder mSB = new System.Text.StringBuilder();

    public static string GetVolumeText(UnitMeasureType type, float volume) {
        mSB.Clear();

        float whole = Mathf.Floor(volume);
        if(volume - whole > 0f)
            mSB.Append(volume.ToString("F2"));
        else
            mSB.Append(volume.ToString("F0"));

        mSB.Append(' ');
        mSB.Append(GetText(type));
        mSB.Append('³');

        return mSB.ToString();
    }

    public static string GetMeasureText(UnitMeasureType type, float val) {
        mSB.Clear();

        float whole = Mathf.Floor(val);
        if(val - whole > 0f)
            mSB.Append(val.ToString("F2"));
        else
            mSB.Append(val.ToString("F0"));

        mSB.Append(' ');
        mSB.Append(GetText(type));

        return mSB.ToString();
    }
}