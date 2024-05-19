# First-Person-Lighting
 Source for the First-Person-Lighting mod for Daggerfall Unity

## Light Utility For Other Modders
The easiest way to get player tint is by checking the Tint property in the FPSWeapon or TransportManager class.
These values are continuously updated by the First-Person-Lighting mod.

The First-Person-Lighting mod has a messaging receiver that can provide additional light information.
Available messages are as follows:
- **entityLighting**: Gets the light level at the location for the specified Daggerfall entity.
- **locationLighting**: Gets the light level at a specified point.
- **playerTint**: Basically the same as entityLighting for the PC, but includes magical concealment effects.
- **gropeLightRange**: returns light radius of the PC grope light.

**Example**
```
        Color GetEntityLighting(DaggerfallEntityBehaviour entity)
        {
            Color lighting = Color.white;

            Mod firstPersonLightingMod = ModManager.Instance.GetMod("First-Person-Lighting");

            if (firstPersonLightingMod != null || firstPersonLightingMod.IsReady)
            {
                 firstPersonLightingMod.MessageReceiver("entityLighting", entity, (string message, object data) =>
                 {
                     lighting = (Color)data;
                 });
            }

            return lighting;
        }
```
