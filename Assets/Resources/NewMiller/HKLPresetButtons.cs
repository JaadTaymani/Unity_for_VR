using UnityEngine;

public class HKLPresetButtons : MonoBehaviour
{
    public MillerPlaneInCell millerPlane;

    public void Set100() => millerPlane.SetHKL(1, 0, 0);
    public void Set010() => millerPlane.SetHKL(0, 1, 0);
    public void Set001() => millerPlane.SetHKL(0, 0, 1);

    public void Set110() => millerPlane.SetHKL(1, 1, 0);
    public void Set101() => millerPlane.SetHKL(1, 0, 1);
    public void Set011() => millerPlane.SetHKL(0, 1, 1);

    public void Set111() => millerPlane.SetHKL(1, 1, 1);
}
