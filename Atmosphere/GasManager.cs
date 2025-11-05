using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Pandora1337.Atmosphere.State;

namespace Pandora1337.Atmosphere;

public partial class GasManager : Node
{
    //this class handles everything gas and atmosphere related
    [ExportCategory("Simulation")]
    [Export] public bool isContinuous = true;
    /// <summary>
    /// Wind will only flow to lower pressure cells
    /// </summary>
    [Export] public bool onlyOutflow = false;

    enum NeighborNumber { FOUR, EIGHT }
    [Export] NeighborNumber neighborMode;

    [ExportCategory("Diffusion")]
    [Export] bool useSimpleDiffusion = false;
    [Export] float diffusionRate = 0.1f;

    /// <summary> values below will delete gas cell</summary>
    [Export] float minConcentration = 0.001f;

    [ExportCategory("Coefs")]
    [Export] float thermalConductivity = 0.02f;
    [Export] float windForceCoef = 1f;

    // TODO make these private
    public TileMapLayer _tilemap;

    public System.Collections.Generic.Dictionary<Vector2I, GasMix> gasDict = [];
    private HashSet<Vector2I> wallDict = [];

    private GasVisualiser gv;
    public static GasManager instance;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _tilemap = GetNodeOrNull<TileMapLayer>("../TileMap");
        if (_tilemap == null)
            GD.PrintErr("TileMap Missing!");

        gv = GetNode<GasVisualiser>("GasVisualiser");

        // Scan tilemap for walls and add their positions to a hashset
        foreach (var tilePos in _tilemap.GetUsedCells())
        {
            // // Optional tile metadata
            // TileData tileData = _tilemap.GetCellTileData(tilePos);
            // if (tileData != null && (bool)tileData.GetCustomData("isSolid"))
            //     wallDict.Add(tilePos);
            wallDict.Add(tilePos);
        }

        instance = this;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!isContinuous)
            return;

        // if (Engine.GetPhysicsFrames() % 10 != 0)
        //     return;

        Step();
    }

    public void Step()
    {
        if (gasDict.Count == 0)
            return;

        System.Collections.Generic.Dictionary<Vector2I, GasMix> copyDict = gasDict.ToDictionary();
        foreach ((Vector2I aPos, GasMix a) in copyDict)
        {
            // not enough gas, delete cell
            if (a.Moles < minConcentration || a.Gases.Count == 0)
            {
                RemoveGasCell(aPos);
                continue;
            }

            // see if neighbor tiles can be expanded to
            Array<Vector2I> neighbours = [];
            foreach (Vector2I pos in GetNeighbourCells(aPos))
            {
                // check for non-solid cell
                if (!wallDict.Contains(pos))
                    neighbours.Add(pos);
            }

            // Fancy Diffusion
            if (!useSimpleDiffusion)
                DiffusionAccurate(a, neighbours);

            Vector2 totalWind = new();
            for (int i = 0; i < neighbours.Count; i++)
            {
                Vector2I nPos = neighbours[i];

                if (useSimpleDiffusion)
                    Diffusion(a, nPos);

                GasMix n = gasDict[nPos];

                // Temp
                Conductivity(a, n);

                // Wind
                totalWind += GetWind(a.Pressure, aPos, n.Pressure, nPos);

                // Advection
                // Move moles according to the wind
            }

            a.Wind = totalWind;
            a.RecalculatePressure();
        }
        // TODO update visuals from here
    }

    /// <summary>
    /// Simple diffusion model, does not account for total moles out of a cell,
    /// and does not balance neighbors
    /// </summary>
    /// <param name="source">Source gas cell, a</param>
    /// <param name="nPos">Position of the neighbor</param>
    private void Diffusion(GasMix source, Vector2I nPos)
    {
        foreach (GasData gasData in source.Gases)
        {
            // is there a gas cell?
            if (gasDict.TryGetValue(nPos, out GasMix n))
            {
                // Is there this gas?
                float destinationMoles = n.TryGetGasData(gasData.gas, out GasData destGasData) ? destGasData.moles : 0;
                float moleDiff = diffusionRate * (gasData.moles - destinationMoles);

                // if (moleDiff <= Gas.MIN_PRESSURE_DIFFERENCE / 10)
                //     continue;

                gasData.moles -= moleDiff;
                n.AddGas(gasData.gas, moleDiff);
            }
            else
            {
                var moleDiff = diffusionRate * gasData.moles;
                gasData.moles -= moleDiff;
                CreateGasCell(nPos, gasData.gas, moleDiff);
            }
        }
    }

    void DiffusionAccurate(GasMix a, Array<Vector2I> neighbors)
    {
        // Fancy Diffusion
        foreach (GasData aData in a.Gases)
        {
            Array<float> nMoleDiffs = [];
            Array<Vector2I> lowerNeighbours = [];
            float nTotal = 0f;
            foreach (Vector2I nPos in neighbors)
            {
                if (gasDict.TryGetValue(nPos, out GasMix n))
                {
                    float nMoles = n.TryGetGasData(aData.gas, out GasData nData) ? nData.moles : 0;
                    if (aData.moles <= nMoles)
                        continue;

                    nMoleDiffs.Add(aData.moles - nMoles);
                    lowerNeighbours.Add(nPos);
                    nTotal += nMoles;
                }
                else
                {
                    nMoleDiffs.Add(aData.moles);
                    lowerNeighbours.Add(nPos);
                    CreateGasCell(nPos, aData.gas);
                }
            }

            var aAverage = (aData.moles + nTotal) / (lowerNeighbours.Count + 1);
            var aDelta = (aData.moles - aAverage) * diffusionRate;
            var nMoleDiff = nMoleDiffs.Sum();

            for (int i = 0; i < lowerNeighbours.Count; i++)
            {
                var nDelta = aDelta * nMoleDiffs[i] / nMoleDiff;
                gasDict[lowerNeighbours[i]].AddGasWithTemperature(aData.gas, nDelta, a.Temperature);
            }
            aData.moles -= aDelta;
        }
    }

    private void Conductivity(GasMix source, GasMix destination)
    {
        if (destination.Temperature > source.Temperature)
            return;

        float deltaT = thermalConductivity * (source.Temperature - destination.Temperature);

        source.Temperature -= deltaT;
        destination.Temperature += deltaT;
    }

    private Vector2 GetWind(float aPressure, Vector2 aPos, float nPressure, Vector2 nPos)
    {
        if (onlyOutflow && nPressure > aPressure)
            return Vector2.Zero;

        Vector2 nDir = nPos - aPos;
        return windForceCoef * (aPressure - nPressure) * nDir;
    }

    private IEnumerable<Vector2I> GetNeighbourCells(Vector2I parentPos)
    {
        switch (neighborMode)
        {
            case NeighborNumber.EIGHT:
                // 8 Cell neighborhood
                yield return parentPos + Vector2I.Right;
                yield return parentPos + Vector2I.Right + Vector2I.Down;
                yield return parentPos + Vector2I.Down;
                yield return parentPos + Vector2I.Down + Vector2I.Left;
                yield return parentPos + Vector2I.Left;
                yield return parentPos + Vector2I.Left + Vector2I.Up;
                yield return parentPos + Vector2I.Up;
                yield return parentPos + Vector2I.Up + Vector2I.Right;
                break;

            default:
                // 4 Cell neighborhood
                yield return parentPos + Vector2I.Right;
                yield return parentPos + Vector2I.Down;
                yield return parentPos + Vector2I.Left;
                yield return parentPos + Vector2I.Up;
                break;
        }
    }

    private void CreateGasCell(Vector2I pos, GasObject gas, float moles = 0f)
    {
        GasMix newMix = new();
        newMix.SetMoles(gas, moles);
        gasDict.Add(pos, newMix);
    }

    public void AddGasMixToCell(Vector2I pos, GasMix gasMix)
    {
        if (!gasDict.TryGetValue(pos, out GasMix targetGas))
        {
            CreateGasCell(pos, gasMix.Gases[0].gas);
            targetGas = gasDict[pos];
        }

        foreach (var gas in gasMix.Gases)
        {
            targetGas.AddGasWithTemperature(gas.gas, gas.moles, gasMix.Temperature);
        }
    }

    public void AddGasToCell(Vector2I pos, GasObject gas, float moles, float temperatureK)
    {
        if (!gasDict.TryGetValue(pos, out GasMix targetGas))
        {
            CreateGasCell(pos, gas);
            targetGas = gasDict[pos];
        }

        targetGas.AddGasWithTemperature(gas, moles, temperatureK);
    }

    public bool TryGetGas(Vector2I pos, out GasMix gas)
    {
        return gasDict.TryGetValue(pos, out gas);
    }

    public void RemoveGasCell(Vector2I pos)
    {
        gasDict.Remove(pos);
        // TODO add a signal for cell removal
        gv.RemoveTile(pos);
    }

    public void AddWallCell(Vector2I pos)
    {
        wallDict.Add(pos);
    }

    public IEnumerable<Vector2I> GetWalls()
    {
        return wallDict.AsEnumerable();
    }

    public void RemoveWallCell(Vector2I pos)
    {
        wallDict.Remove(pos);
    }

    public Vector2I GetGridCoord(Vector2 pos)
    {
        Vector2 mapPos = _tilemap.ToLocal(pos);
        Vector2I posTile = _tilemap.LocalToMap(mapPos);

        return posTile;
    }
}
