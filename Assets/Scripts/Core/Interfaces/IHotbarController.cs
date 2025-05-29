namespace GameFramework.Core.Interfaces
{
    public interface IHotbarController
    {
        void UpdateHotbarSlot(int slotIndex, object itemStack);
        void RefreshHotbar();
        object GetHotbarItem(int slotIndex);
        int GetHotbarSize();
        bool AddItemToHotbar(object item);
        int FindItemInHotbar(object item);
        void SelectHotbarSlot(int slotIndex);
    }
}