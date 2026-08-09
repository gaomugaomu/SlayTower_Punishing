# BattleManager Specification

Responsibilities: - Initialize battle - Control battle lifecycle -
Coordinate systems - Determine victory and defeat

BattleManager must not contain: - Character skill logic - Damage
formulas - UI code

Suggested interface:

IBattleManager - StartBattle() - EndBattle() - GetBattleState()
