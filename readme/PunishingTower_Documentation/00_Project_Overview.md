# Punishing Tower - Project Overview

## Project Vision

A 3D character driven roguelike strategy game combining: - Slay the
Spire style tower climbing and deck building - Punishing Gray Raven
inspired signal orb combat - 3D character skill presentation

## Player Identity

The player is the Commander. Constructs are combat units protecting the
Commander.

Commander owns: - HP - Infection - Serum/Potion inventory

Commander HP reaches zero = run failure.

## Core Combat

The combat system is turn based. The core decisions are: 1. Enemy intent
2. Signal orb ordering 3. Three match planning 4. Construct skill
allocation 5. Action point management 6. Ultimate timing

## Technical Rules

-   Unity + C#
-   Data driven architecture
-   ScriptableObject based game data
-   Event driven combat system
-   No ARPG movement combat
-   No construct HP system
