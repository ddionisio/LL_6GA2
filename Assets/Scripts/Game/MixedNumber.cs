using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MixedNumber : System.IComparable, System.IComparable<MixedNumber> {
    [SerializeField]
    bool _negative;
    [SerializeField]
    int _whole;
    [SerializeField]
    int _numerator;
    [SerializeField]
    int _denominator;
    
    public int whole {
        get { return _whole; }
        set {
            _negative = value < 0;
            _whole = Mathf.Abs(value);
        }
    }

    public int numerator {
        get { return _numerator; }
        set {
            _negative = value < 0;
            _numerator = Mathf.Abs(value);
        }
    }

    public int denominator {
        get { return _denominator; }
        set {
            _negative = value < 0;
            _denominator = Mathf.Abs(value);
        }
    }

    public bool isValid { get { return _numerator == 0 || _denominator != 0; } }

    public bool isNegative { get { return _negative; } set { _negative = value; } }

    public float sign { get { return isNegative ? -1f : 1f; } }

    public float fValue {
        get {
            if(_denominator == 0) //fail-safe
                return sign * _whole;

            return sign * (_whole + (float)_numerator / _denominator);
        }

        set {
            _negative = value < 0f;

            var val = Mathf.Abs(value);

            var s = val.ToString();

            var numDigits = s.Length - 1 - s.IndexOf('.');
            var denom = 1;

            for(int i = 0; i < numDigits; i++) {
                val *= 10f;
                denom *= 10;
            }

            var num = Mathf.RoundToInt(val);
            int g = M8.MathUtil.Gcf(num, denom);

            _numerator = num / g;
            _denominator = denominator / g;
        }
    }

    public MixedNumber simplified {
        get {
            var ret = this;
            ret.Simplify();
            return ret;
        }
    }

    /// <summary>
    /// Convert only one _whole number to fraction. E.g. [2 1/2] => [1 3/2]
    /// </summary>
    public void WholeToFractionSingle() {
        if(_whole == 0)
            return;

        if(_denominator == 0) {
            _denominator = 1;
        }

        _numerator += _denominator;
        _whole--;
    }

    /// <summary>
    /// Convert only a single _whole number from _numerator. E.g. [5/2] => [1 3/2]
    /// </summary>
    public void FractionToWholeSingle() {
        if(_whole == 0)
            return;
        
        _numerator += _denominator;
        _whole--;
    }

    /// <summary>
    /// Convert _whole number to fraction. E.g. [2 1/2] => [5/2]
    /// </summary>
    public void WholeToFraction() {
        if(_denominator > 0)
            _numerator += _whole * _denominator;
        else {
            _numerator += _whole;
            _denominator = 1;
        }

        _whole = 0;
    }

    public int GetWholeFromFraction() {
        return _numerator >= _denominator ? Mathf.FloorToInt((float)_numerator / _denominator) : 0;
    }

    public int GetGreatestCommonFactor() {
        return M8.MathUtil.Gcf(_numerator, _denominator);
    }

    /// <summary>
    /// Convert fraction to _whole number. E.g. [5/2] => [2 1/2]
    /// </summary>
    public void FractionToWhole() {
        var amt = GetWholeFromFraction();
        if(amt > 0) {
            _whole += amt;
            _numerator -= amt * _denominator;
        }
    }

    /// <summary>
    /// Convert _numerators to _whole, and simplify fraction. E.g. [6/4] => [1 1/2]
    /// </summary>
    public void Simplify() {
        if(_numerator > _denominator) {
            int amt = Mathf.FloorToInt((float)_numerator / _denominator);
            _whole += amt;
            _numerator -= amt * _denominator;
        }

        int gcf = GetGreatestCommonFactor();

        _numerator /= gcf;
        _denominator /= gcf;
    }

    /// <summary>
    /// Apply whole to numerator, then simplify
    /// </summary>
    public void SimplifyImproper() {
        WholeToFraction();

        int gcf = GetGreatestCommonFactor();

        _numerator /= gcf;
        _denominator /= gcf;
    }
        
    public int CompareTo(MixedNumber other) {
        if(fValue < other.fValue)
            return -1;
        else if(fValue > other.fValue)
            return 1;

        return 0;
    }
    
    public override int GetHashCode() {
        return fValue.GetHashCode();
    }

    public override bool Equals(object obj) {
        if(obj is MixedNumber) {
            return fValue == ((MixedNumber)obj).fValue;
        }

        return false;
    }

    public override string ToString() {
        var strBuff = new System.Text.StringBuilder();
        ApplyString(strBuff);

        return strBuff.ToString();
        
        //return string.Format("{0} {1} and ({2}/{3})", _negative ? "-" : "", _whole, _numerator, _denominator);
    }

    public void ApplyString(System.Text.StringBuilder strBuff) {
        if(!isValid)
            strBuff.Append("INVALID");

        if(_negative)
            strBuff.Append('-');

        if(_whole > 0)
            strBuff.Append(_whole).Append(' ');

        if(_numerator > 0 && _denominator > 0) {
            if(_numerator == _denominator)
                strBuff.Append('1');
            else if(_denominator == 1)
                strBuff.Append(_numerator);
            else
                strBuff.Append(_numerator).Append('/').Append(_denominator);
        }
        else if(_whole == 0)
            strBuff.Append('0');
    }

    int System.IComparable.CompareTo(object obj) {
        if(obj is MixedNumber) {
            var other = (MixedNumber)obj;
            return CompareTo(other);
        }

        return -1;
    }

    public static MixedNumber operator +(MixedNumber a, MixedNumber b) {
        if(!a.isValid)
            return b;
        else if(!b.isValid)
            return a;

        a.WholeToFraction();
        b.WholeToFraction();

        MixedNumber result;

        var aNumerator = a.isNegative ? -a.numerator : a.numerator;
        var bNumerator = b.isNegative ? -b.numerator : b.numerator;

        if(a.denominator == b.denominator)
            result = new MixedNumber { numerator = aNumerator + bNumerator, denominator = a.denominator };
        else {
            result = new MixedNumber { numerator = aNumerator * b.denominator + bNumerator * a.denominator, denominator = a.denominator * b.denominator };
        }

        return result;
    }

    public static MixedNumber operator -(MixedNumber a, MixedNumber b) {
        if(!a.isValid)
            return -b;
        else if(!b.isValid)
            return a;

        a.WholeToFraction();
        b.WholeToFraction();

        MixedNumber result;

        var aNumerator = a.isNegative ? -a.numerator : a.numerator;
        var bNumerator = b.isNegative ? -b.numerator : b.numerator;

        if(a.denominator == b.denominator)
            result = new MixedNumber { numerator = aNumerator - bNumerator, denominator = a.denominator };
        else {
            result = new MixedNumber { numerator = aNumerator * b.denominator - bNumerator * a.denominator, denominator = a.denominator * b.denominator };
        }

        return result;
    }

    public static MixedNumber operator-(MixedNumber a) {
        a.isNegative = !a.isNegative;
        return a;
    }

    public static MixedNumber operator *(MixedNumber a, MixedNumber b) {
        if(!a.isValid || !b.isValid)
            return new MixedNumber { _whole = 0 };

        a.WholeToFraction();
        b.WholeToFraction();

        return new MixedNumber {
            _negative = a.isNegative && b.isNegative ? false : a.isNegative || b.isNegative,
            _numerator = a._numerator*b._numerator,
            _denominator = a._denominator*b._denominator };
    }

    public static MixedNumber operator *(MixedNumber a, int b) {
        if(!a.isValid)
            return new MixedNumber { _whole = 0 };

        a.WholeToFraction();

        return new MixedNumber {
            _negative = a.isNegative && b < 0 ? false : a.isNegative || b < 0,
            _numerator = a._numerator * b,
            _denominator = a._denominator
        };
    }

    public static MixedNumber operator *(int b, MixedNumber a) {
        if(!a.isValid)
            return new MixedNumber { _whole = 0 };

        a.WholeToFraction();

        return new MixedNumber {
            _negative = a.isNegative && b < 0 ? false : a.isNegative || b < 0,
            _numerator = a._numerator * b,
            _denominator = a._denominator
        };
    }

    public static bool operator ==(MixedNumber a, MixedNumber b) {
        return a.fValue == b.fValue;
    }

    public static bool operator !=(MixedNumber a, MixedNumber b) {
        return a.fValue != b.fValue;
    }

    public static bool operator <(MixedNumber a, MixedNumber b) {
        return a.fValue < b.fValue;
    }

    public static bool operator <=(MixedNumber a, MixedNumber b) {
        return a.fValue <= b.fValue;
    }

    public static bool operator >(MixedNumber a, MixedNumber b) {
        return a.fValue > b.fValue;
    }

    public static bool operator >=(MixedNumber a, MixedNumber b) {
        return a.fValue >= b.fValue;
    }
}

[System.Serializable]
public class MixedNumberGroup {
    [SerializeField]
    MixedNumber[] numbers = null;
    [SerializeField]
    bool isShuffle = false;

    public MixedNumber[] GetNumbers() {
        if(isShuffle)
            M8.ArrayUtil.Shuffle(numbers);
        return numbers;
    }
}