using System;

public static class GameEvents
{
#region Pickups
    public static event Action<IPickup> OnPickup;
    public static event Action<IPickup> OnUse;
    public static event Action<IPickup> OnDrop;

    public static void ExecOnPickup(IPickup item)
    {
        OnPickup?.Invoke(item);
    }
    
    public static void ExecOnUse(IPickup item)
    {
        OnUse?.Invoke(item);
    }

    public static void ExecOnDrop(IPickup item)
    {
        OnDrop?.Invoke(item);
    }
#endregion
}