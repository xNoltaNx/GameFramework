using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract { get; }
        
        void Interact(GameObject interactor);
        void OnInteractionEnter(GameObject interactor);
        void OnInteractionExit(GameObject interactor);
    }
}