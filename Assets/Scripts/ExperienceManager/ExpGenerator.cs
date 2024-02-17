using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExpGenerator {
    public static float GetSigmoid(int level, float constantration, float shift) {
        float expTerm = (float) Math.Exp(level - shift);
        float denominator = expTerm + 1;
        return constantration * (expTerm / denominator) + 1;
    }

    public static float GetLog(float level, float @base = 2f) {
        level = level <= 1 ? 2 : level;
        return MathF.Log(level, @base);
    }

    public static float GetExponential(float @base, float power = 2) {
        if ((@base == 0 && power == 0) || (@base <= 0 && power <= 0) || @base <= 0 || power <= 0) 
            return 0;
        else return MathF.Pow(@base, power);
    }

    public static float GetLinear(float number, float multiplier = 1) {
        return number * multiplier;
    }

}
