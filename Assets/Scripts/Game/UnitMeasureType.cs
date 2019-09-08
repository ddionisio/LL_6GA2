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

    public static string GetVolumeText(UnitMeasureType type) {
        return GetText(type) + '³';
    }

    public static string GetNumberFormatted(float val) {
        float whole = Mathf.Floor(val);
        if(val - whole > 0f)
            return val.ToString("F3");
        else
            return val.ToString("F0");
    }

    public static string GetVolumeText(UnitMeasureType type, float volume) {
        mSB.Clear();

        mSB.Append(GetNumberFormatted(volume));
        //mSB.Append(' ');
        mSB.Append(GetText(type));
        mSB.Append('³');

        return mSB.ToString();
    }

    public static string GetMeasureText(UnitMeasureType type, float val) {
        mSB.Clear();

        mSB.Append(GetNumberFormatted(val));
        //mSB.Append(' ');
        mSB.Append(GetText(type));

        return mSB.ToString();
    }
}