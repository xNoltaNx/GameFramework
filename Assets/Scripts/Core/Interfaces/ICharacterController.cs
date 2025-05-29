namespace GameFramework.Core.Interfaces
{
    public interface ICharacterController
    {
        IInventoryController GetInventoryController();
        IEquipmentController GetEquipmentController();
        IInteractionController GetInteractionController();
    }
}