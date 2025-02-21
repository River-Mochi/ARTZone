using System.Collections.Generic;
using System.Linq;
using Game.Net;

namespace AdvancedRoadTools;

/// <summary>
/// A collection of curves.
/// </summary>
/// <param name="curves">Collection of curves that compose this ComplexCurve</param>
public struct ComplexCurve(IEnumerable<Curve> curves)
{
    public List<Curve> curves = [..curves];

    /// <summary>
    /// Length of all the curves combined.
    /// </summary>
    public float Length
    {
        get
        {
            return curves.Sum(curve => curve.m_Length);
        }
    }
    
    public Curve this[int index] => curves[index];
}