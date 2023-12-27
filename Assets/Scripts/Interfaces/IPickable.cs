// Use this interface for components that the player can use
public interface IPickable
{
    public bool CanBePickedUp();

    public void SetCanBePickedUp(bool value);

    void PickUpItem();
}