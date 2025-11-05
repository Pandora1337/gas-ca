using Godot;
using Godot.Collections;
using System;

namespace Pandora1337.Atmosphere;

public partial class GasMix
{
    public Array<GasData> Gases;

    public float Volume;

    private float pressure;

    /// <summary>
    /// Pa (Pascals)
    /// </summary>
    public float Pressure
    {
        get => pressure;
        set
        {
            if (float.IsNormal(value) == false && value != 0)
            {
                GD.PrintErr($"pressure Invalid number! {value}");
                return;
            }

            pressure = Math.Clamp(value, 0, Single.MaxValue);
        }
    }

    private float temperature;
    /// <summary>
    /// In Kelvin
    /// </summary>
    public float Temperature
    {
        get => temperature;
        set
        {
            if (float.IsNormal(value) == false && value != 0)
            {
                GD.PrintErr($"Temperature Invalid number! {value}");
                return;
            }

            temperature = Math.Clamp(value, 0, Single.MaxValue);
        }
    }

    public float Moles
    {
        get
        {
            float value = 0;
            foreach (var gas in Gases)
            {
                value += gas.moles;
            }

            if (float.IsNaN(value))
            {
                return 0;
            }

            return value;
        }
    }

    public Vector2 Wind;

    public void RecalculatePressure()
    {
        Pressure = Gas.CalcPressure(Volume, Moles, Temperature);
    }

    public void SetTemperature(float newTemp)
    {
        Temperature = newTemp;
        RecalculatePressure();
    }

    public void ChangeMoles(GasObject gas, float deltaMoles)
    {
        InternalSetMoles(gas, deltaMoles, true);
    }

    public void ChangeMoles(float deltaMoles)
    {
        foreach (GasData gasData in Gases)
        {
            InternalSetMoles(gasData.gas, deltaMoles / Gases.Count, true);
        }
    }

    public void SetMoles(GasObject gas, float newMoles)
    {
        InternalSetMoles(gas, newMoles, false);
    }

    public void SetMoles(float deltaMoles)
    {
        foreach (GasData gasData in Gases)
        {
            InternalSetMoles(gasData.gas, deltaMoles / Gases.Count, false);
        }
    }

    private void InternalSetMoles(GasObject gasType, float moles, bool isChange)
    {
        //Try to get gas value if already inside mix
        if (this.TryGetGasData(gasType, out GasData gasData))
        {
            gasData.moles = isChange ? moles + gasData.moles : moles;

            //Remove gas from mix if less than threshold
            if (gasData.moles <= Gas.MIN_PRESSURE_DIFFERENCE)
            {
                Gases.Remove(gasData);
            }
            return;
        }

        //Gas isn't inside mix so add it

        //Dont add new data for negative moles
        if (moles <= 0)
            return;

        //Dont add if approx 0 or below threshold
        if (Mathf.IsEqualApprox(moles, 0) || moles <= Gas.MIN_PRESSURE_DIFFERENCE)
            return;

        Gases.Add(new GasData { gas = gasType, moles = moles });
    }

    public void AddGasWithTemperature(GasObject gas, float moles, float kelvinTemperature)
    {
        // Tf = (n1 * T1 + n2 * T2) / (n1 + n2)
        Temperature = (Moles * Temperature + moles * kelvinTemperature) / (Moles + moles);
        AddGas(gas, moles);
    }

    public void AddGas(GasObject gas, float moles)
    {
        ChangeMoles(gas, moles);
        RecalculatePressure();
    }

    public void RemoveGas(GasObject gas, float moles)
    {
        ChangeMoles(gas, -moles);
        RecalculatePressure();
    }

    /// <summary>
    /// Get an average color of all the (visible) gases in a cell
    /// </summary>
    public Color GetGasColor()
    {
        Color gasColor = new();
        int numVisibleGases = 0;
        for (int i = 0; i < Gases.Count; i++)
        {
            if (!Gases[i].gas.customColor || Gases[i].moles < Gases[i].gas.minMolesToSee)
                continue;

            float ratio = Gases[i].moles / Moles;
            Color nextGasCol = Gases[i].gas.color * ratio;
            gasColor += nextGasCol;

            numVisibleGases++;
        }

        gasColor /= numVisibleGases;

        return gasColor;
    }

    public bool TryGetGasData(GasObject gasType, out GasData targetGasData)
    {
        foreach (GasData gasData in Gases)
        {
            if (gasData.gas != gasType)
                continue;

            targetGasData = gasData;
            return true;
        }
        targetGasData = null;
        return false;
    }

    public bool Contains(GasObject gasType)
    {
        foreach (GasData gasData in Gases)
        {
            if (gasData.gas == gasType)
                return true;
        }
        return false;
    }

    public GasMix Copy()
    {
        GasMix newGasMix = new();
        foreach (GasData gas in Gases)
        {
            newGasMix.SetMoles(gas.gas, gas.moles);
        }
        newGasMix.Temperature = Temperature;
        return newGasMix;
    }

    public GasMix()
    {
        Gases = [];
        Volume = Gas.TILE_VOLUME;
        temperature = Gas.SPACE_TEMP;
    }
}