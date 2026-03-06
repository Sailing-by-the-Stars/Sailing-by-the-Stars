using UnityEngine;

public class TorchPickup : PhysicsPickup
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