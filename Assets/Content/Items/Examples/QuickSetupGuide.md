# Equipment Ability System - Quick Setup Guide

## 🚀 Quick Start (5 minutes)

### Step 1: Create the Double Jump Template
1. Right-click in Project → `Create` → `GameFramework` → `Equipment Abilities` → `Double Jump`
2. Name it `DoubleJumpTemplate`
3. Configure settings:
   - **Double Jump Height Multiplier**: `0.8` (80% of normal jump)
   - **Reset On Wall Contact**: `✓` (enables wall-kick mechanics)
   - **Double Jump Sound**: Assign an audio clip (optional)

### Step 2: Create the Boots Item
1. Right-click in Project → `Create` → `GameFramework` → `Items` → `Equippable Item`
2. Name it `DoubleJumpBoots`
3. Configure the item:
   ```
   Item Name: "Double Jump Boots"
   Description: "Mystical boots that allow double jumping"
   Equipment Slot: Feet
   Weight: 2
   Value: 150
   Ability Templates: [Drag DoubleJumpTemplate here]
   ```

### Step 3: Verify Player Setup
Your player GameObject needs these components:
- `FirstPersonLocomotionController` ✓
- `EquipmentController` ✓  
- `PlayerInputHandler` ✓
- `CharacterController` ✓
- `AudioSource` (for ability sounds)

### Step 4: Test It!
1. **Add to Inventory**: Drag `DoubleJumpBoots` into your inventory
2. **Equip**: Move boots to the feet equipment slot
3. **Test**: Jump normally, then press jump again in mid-air → Double jump!

---

## 🔧 Player GameObject Setup

### Required Components Configuration

```csharp
// EquipmentController settings
[SerializeField] private bool autoFindAttachmentPoints = true;
[SerializeField] private bool debugMode = true; // Enable for testing

// Attachment points (optional for visual equipment)
[SerializeField] private Transform feetAttachment; // For boot visuals
```

### Input System Integration
The `PlayerInputHandler` automatically detects and notifies `DoubleJumpAbility` components when jump is pressed. No additional setup needed!

---

## 🎮 Testing in Play Mode

### Debug Information
With `debugMode = true` on EquipmentController, you'll see:
```
Double Jump ability equipped!
Added ability component: DoubleJumpAbility
Double jump performed!
```

### Visual Debugging
Select the player in Scene view to see:
- **Cyan sphere**: Can double jump
- **Red sphere**: Double jump used
- **Green sphere**: On ground (double jump reset)
- **Yellow ray**: Wall detection for reset

---

## 🏗️ Creating New Abilities

### 1. Create Ability Component
```csharp
public class MyAbility : MonoBehaviour, IEquipmentAbility
{
    public bool IsActive { get; private set; }
    
    public void OnEquipped(GameObject equipper)
    {
        IsActive = true;
        // Setup logic
    }
    
    public void OnUnequipped(GameObject equipper)
    {
        IsActive = false;
        // Cleanup logic
    }
}
```

### 2. Create Template
```csharp
[CreateAssetMenu(fileName = "MyAbilityTemplate", menuName = "GameFramework/Equipment Abilities/My Ability")]
public class MyAbilityTemplate : ScriptableObject, IEquipmentAbility
{
    public MyAbility CreateAbilityComponent(GameObject target)
    {
        return target.AddComponent<MyAbility>();
    }
    
    // Template properties only (IsActive always false)
    public bool IsActive => false;
    public void OnEquipped(GameObject equipper) { }
    public void OnUnequipped(GameObject equipper) { }
}
```

### 3. Register in EquipmentController
Add to `AddAbilityComponents()` method:
```csharp
else if (template is MyAbilityTemplate myTemplate)
{
    newComponent = myTemplate.CreateAbilityComponent(gameObject);
}
```

---

## 🚨 Troubleshooting

### "Double jump not working"
- ✅ Check boots are equipped to **Feet** slot
- ✅ Verify `DoubleJumpAbilityTemplate` is assigned to boots
- ✅ Enable `debugMode` on `EquipmentController` for logs
- ✅ Ensure player has `FirstPersonLocomotionController`

### "No ability component added"
- ✅ Check console for error messages
- ✅ Verify template is created correctly
- ✅ Ensure `EquipmentController` has the abilities namespace imported

### "Jump input not detected"
- ✅ Verify `PlayerInputHandler` is on same GameObject as abilities
- ✅ Check Input System actions are properly configured
- ✅ Test normal jumping first

---

## 🎯 Advanced Usage

### Runtime Ability Queries
```csharp
// Check if player has specific ability
var doubleJump = player.GetComponent<DoubleJumpAbility>();
if (doubleJump?.IsActive == true)
{
    bool canJump = doubleJump.CanDoubleJump;
    bool hasUsed = doubleJump.HasDoubleJumped;
}
```

### Ability Interactions
```csharp
// Abilities can interact with each other
var dash = GetComponent<DashAbility>();
var doubleJump = GetComponent<DoubleJumpAbility>();

if (dash?.IsActive == true && doubleJump?.CanDoubleJump == true)
{
    // Combine abilities for special moves
}
```

### Equipment Requirements
```csharp
// Check equipment before allowing actions
var equipmentController = GetComponent<EquipmentController>();
if (equipmentController.IsSlotOccupied("Feet"))
{
    var boots = equipmentController.GetEquippedItem("Feet");
    // Do something based on equipped boots
}
```