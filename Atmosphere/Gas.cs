using System;
using Godot;

namespace Pandora1337.Atmosphere;

public static class Gas
{
    // Gas constant
    public const float R = 8.3144598f;
    public const float TILE_VOLUME = 1f;
    public const float SPACE_TEMP = 2.7f;
    public const float MIN_PRESSURE_DIFFERENCE = 0.0001f;

    public static float CalcTemperature(float pressure, float volume, float moles)
    {
        if (volume > 0 && pressure > 0 && moles > 0)
        {
            return pressure * volume / (R * moles) * 1000;
        }

        return SPACE_TEMP; //space radiation
    }

    public static float CalcPressure(float volume, float moles, float temperature)
    {
        if (temperature > 0 && moles > 0 && volume > 0)
        {
            return moles * R * temperature / volume / 1000;
        }

        return 0;
    }

    public static float CalcMoles(float pressure, float volume, float temperature)
    {
        if (temperature > 0 && pressure > 0 && volume > 0)
        {
            return pressure * volume / (R * temperature) * 1000;
        }

        return 0;
    }

    public static float CalcVolume(float pressure, float moles, float temperature)
    {
        if (temperature > 0 && pressure > 0 && moles > 0)
        {
            return moles * R * temperature / pressure;
        }

        return 0;
    }

    public static float KToCelsius(float k)
    {
        return k + 273.15f;
    }

    public static float CelsiusToK(float celsius)
    {
        return celsius - 273.15f;
    }
}