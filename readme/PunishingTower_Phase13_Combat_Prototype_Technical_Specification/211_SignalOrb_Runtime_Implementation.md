# Signal Orb Runtime Implementation

Signal orb lifecycle:

Orb Pool

-\> Draw

-\> Hand

-\> Selected

-\> Used

-\> Discard

-\> Exhaust

Implementation:

OrbData: Static definition.

OrbInstance: Runtime object.

OrbManager: Controls drawing and returning.

Must support: - Retain - Top deck - Exhaust - Special modifiers
