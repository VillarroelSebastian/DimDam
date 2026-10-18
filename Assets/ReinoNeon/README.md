# The Neon Core — Circuitos cruzados

Scene: `Assets/ReinoNeon/Scenes/ReinoNeon.unity` (Unity 6000.6.5f1).

Open the scene and play, or choose **N / THE NEON CORE** on the existing world map. The completed Bubblegum Kingdom cooperative exit also loads ReinoNeon. The old serialized `Level_TheNeonCore` destination is migrated at the point of travel. Completion remains visible and **M** or **VOLVER AL MAPA** returns to the registered Candy world map.

## Controls and cooperative route

The original Coral/Mimo and Celeste/Pipo prefab instances retain their existing character sprites, animation components, walking, jump and dash settings. Their established Bubblegum size overrides are copied without editing either prefab. Mimo uses A/D, W to jump and S to interact; Pipo uses arrows, Up to jump and Down to interact. Shift / Right Ctrl retain the original dash. Down is reserved for interaction in this scene, matching Bubblegum's convention. R recovers both players; M returns to the map.

1. **Crossed circuits:** Pipo climbs the left approach to the upper route. Each of three pairs of panels opens the opposite player's next gate. Each panel has explicit owner/target references and a visible animated cable. Activations latch so leaving a control never strands a partner. Both players must reach the relay to commit its checkpoint.
2. **Energy relay:** Mimo physically pushes the battery to socket A. This powers a moving lift and a bridge to the upper vent. Only Pipo fits its 1.1-unit clearance. Pipo's terminal reversibly routes circuit B while the battery remains in A. Routing parks the lift above the rail; Mimo then pushes the battery to B. The battery locks and opens the chamber. R restores its deterministic home before the next checkpoint.
3. **Synchronized core:** Mimo starts the temporary bridge, Pipo crosses and reverses power, then Mimo crosses and confirms. Each handoff renews a forgiving 18-second window. Stand Mimo on the left plate and Pipo on the right plate for 2.5 seconds. A timeout resets only this attempt, returning both players to safe chamber positions and preserving the relay. Reactor art progressively illuminates after restoration.
4. **Shared reward:** both characters must approach the pedestal to collect the Friendship Heart fragment. `ReinoNeon.FriendshipFragment` is saved using the same PlayerPrefs convention as Bubblegum's fragment. Both must approach the portal to complete. The level keeps its completed state and offers a real map return instead of a nonexistent next scene.

## Assets and implementation

`Art/` contains 23 programmatically authored pixel PNGs: hull/wall/platform, bridge states, battery/socket, panels, pads, sliding door states, lift, checkpoint, pedestal, heart, portal, spark, technological backdrop and four reactor phases. Imports use 32 PPU, Point filtering, Full Rect, no mipmaps and no compression. Repeating terrain uses tiled SpriteRenderers and fitted BoxCollider2Ds, following Bubblegum's existing approach rather than introducing another tile system. Props use complete fitted sprites. Background layers move subtly with the shared camera.

`Prefabs/` contains reusable circuit panel, heavy battery and energy bridge assets. Scene circuit wiring and player references are serialized; no manual assignment is needed.

Existing systems reused: `GameManager` exploration mode and synthesized sounds, `PlayerController2D`, `PlayerHealth`, `PixelActorVisual`/existing character animation components, `BubblegumHeavy` swept pushing and recovery, `BubblegumFloat`, `BubblegumWorldLabel`, `PixelFont`, `PixelViewport`, `BeamFX`, and `CandyCampaign.Map`.

New scripts in `Runtime/`: `NeonLevel` explicit puzzle/checkpoint/completion state, `NeonDevice` linked endpoint and cable feedback, `NeonLift` moving platform and rider transfer, `NeonCamera` shared horizontal framing based on the existing shared-camera rule, and `NeonFeedback` device presentation. `Editor/NeonBuilder.cs` rebuilds the assembled scene through **Coop Beam / Neon / Build ReinoNeon**. It refuses to discard an unsaved current scene, copies the existing kingdom character overrides, saves ReinoNeon and adds only its build entry. It never saves changes to an existing level.

## Validation

`QA/NeonPlayQA.cs` is an explicit Editor-only Play Mode harness, never saved as a component in the gameplay scene. Attach it to the NeonLevel object while playing to rerun. It creates a temporary keyboard, uses real controls for walking, jumping, linked-gate crossing, heavy pushing, vent movement and timed bridge crossings, and uses explicit placement to isolate mechanism/checkpoint/reward tests. It restores keyboard/input settings and the existing fragment preference afterward.

`QA/PlayMode-results.txt` records all 43 assertions. `QA/Static-results.txt` records script/import/reference checks. Preview PNGs are captured from Unity. Automated coverage includes a failed timed attempt, retry, recovery after a fall, solo reward/exit rejection and successful joint completion. This is automated gameplay verification with isolated placements, not a continuous two-human playthrough or a standalone player build.

The project Test Runner's EditMode run discovered zero tests. A separate existing GumRemix scene-boot PlayMode test was also requested as a regression smoke check; its actual result is recorded in `QA/TestRunner-results.txt`. The full unrelated combat-campaign suite was not run.

Changes are on `codex/reino-neon`. Pre-existing Bubblegum working changes were retained. Integration changes are limited to Bubblegum's next-scene resolution, the map's Neon entry and the appended build entry. Character prefabs, input implementation and existing level scene files were not edited by this work.

Final Arrival preview captures normal scene startup. Completion preview uses explicit player/object placement for visual inspection after the automated gameplay run; it is not an additional continuous playthrough. The final visual pass fits small cable sprites without clipping and lightens background modules; gameplay geometry is unchanged.
