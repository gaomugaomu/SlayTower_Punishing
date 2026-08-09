# Assembly Definition Plan

Use separate assemblies:

Game.Core Game.Combat Game.Construct Game.SignalOrb Game.Tower Game.UI
Game.Presentation Game.Tests

Rules: - Core should not depend on UI. - Combat should not depend on
Presentation. - Data should remain reusable.
