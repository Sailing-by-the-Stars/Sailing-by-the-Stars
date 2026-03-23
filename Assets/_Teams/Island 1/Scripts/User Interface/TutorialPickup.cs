using UnityEngine;

// Author: Edward
// Taken as example from Island 2, but adapted to be used in the tutorial by Sander Kleine
public class TutorialPickup : TempPhysicsPickup
{
    private Light torchLight;

    protected override void Awake()
    {
        base.Awake();
            
        torchLight = GetComponentInChildren<Light>();
    }

    private void Start()
    {
        SetTorchLight(false);
    }

    public override void Use()
    {
        base.Use();

        SetTorchLight(!torchLight.isActiveAndEnabled);
    }

    public override void Drop(PickupController pickupController)
    {
        base.Drop(pickupController);

        SetTorchLight(false);
    }

    private void SetTorchLight(bool isLightOn)
    {
        torchLight.enabled = isLightOn;
    }
}