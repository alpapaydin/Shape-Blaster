using UnityEngine;

public static class Vibration
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    private static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    private static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
    #endif

    public static void Vibrate(long milliseconds)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call("vibrate", milliseconds);
        #elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
        #endif
    }

    public static void VibratePop()
    {
        Vibrate(27);
    }

    public static void VibrateLight()
    {
        Vibrate(40);
    }

    public static void VibrateMedium()
    {
        Vibrate(80);
    }

    public static void VibrateHeavy()
    {
        Vibrate(120);
    }
}
