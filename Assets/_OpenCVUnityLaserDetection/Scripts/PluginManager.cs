using UnityEngine;
using System.Runtime.InteropServices;

public class PluginManager : MonoSingleton<PluginManager>
{
    public override void Init()
    {
        base.Init();
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern int getCoordinateX();

    [DllImport("__Internal")]
    public static extern int getCoordinateY();

    [DllImport("__Internal")]
    public static extern bool didDetectLaser();

    [DllImport("__Internal")]
    public static extern void resetDetectedLaser();
#endif
}
