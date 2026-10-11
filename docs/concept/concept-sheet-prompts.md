# Natureous concept-sheet prompts

Three prompts for the ChatGPT image model, one per dimension (2D, 2.5D, 3D). Each produces one 6-panel iteration sheet in the same shape as the Koshary sheet:

- Panel 1: reference shot with the full in-game HUD.
- Panel 2: the same shot with real-world and game references.
- Panels 3–6: the same shot, each panel changing **one** variable.

Paste one prompt per chat. If the model drifts from the character, re-paste the **Mino** block only.

Sources: `docs/explanation/game-vision.md`, `docs/trello/board.md`, `docs/research/concept-art/cooking-sim-art-direction-research.md`, input code in `Assets/Natureous/InputDevices/`.

---

## Prompt 1: 2D, "Mino Wakes"

```
Create ONE wide image: a 3x2 grid concept-art iteration sheet for a mobile action-adventure game called "Natureous". Thin dark gutters between panels. Each panel has a small white label in its top-left corner exactly as written below. All six panels show the SAME moment, same character and same composition; only the labelled variable changes.

STYLE: 2D side-view, flat hand-painted illustration, clean shapes, soft painterly texture, readable silhouettes, warm earthy palette that feels cozy but slightly eerie. Simple but expressive character design. Leave some empty space so the character reads instantly at phone size.

SCENE: Inside a huge hollow beneath the roots of an ancient world tree, deep in a dark forest. Nature has slept for ages. Mino, the hero, has just been released by Nature and is rising from a glowing pool of sap-green light among twisted roots. Broken, moss-covered stone monuments carved with leaf patterns lie in the background. Fireflies drift. Game camera is side-on; the ground runs left to right.

MINO (keep identical in every panel): lean humanoid nature spirit, about 1.8 m, body plated with dark bark, two antler-like horns curving up from the head, glowing sap-green light at the chest and joints, no clothes except a short ragged moss cloak, calm determined face, no weapon (fights with fists for now).

PANEL LABELS AND VARIABLES:
1. "1 Reference - full HUD": mobile landscape screenshot. Virtual joystick bottom-left, round attack button bottom-right with a fist icon, pause button top-right, small mana bar top-left, objective chip at top center reading "Find the first monument".
2. "2 Target - real forest + refs": no HUD. Same shot, materials grounded in real forest photos: real bark, real moss, real roots, light shafts through canopy. Paint over with the game style.
3. "3 Variant A - 3 parallax layers": no HUD. Background simplified to 3 flat parallax layers.
4. "4 Variant B - 6 parallax layers": no HUD. Background split into 6 parallax layers with atmospheric fog between them.
5. "5 Variant C - Mino 1.3x": no HUD. Same as panel 2, Mino scaled 1.3x relative to the scene.
6. "6 Variant D - Mino 1.6x + outline": no HUD. Mino scaled 1.6x with a thin dark outline for readability.

Do not add any other text, logos or watermarks.
```

## Prompt 2: 2.5D, "The Eastern Gate" (the agreed dimension)

```
Create ONE wide image: a 3x2 grid concept-art iteration sheet for a mobile action-adventure game called "Natureous". Thin dark gutters between panels. Each panel has a small white label in its top-left corner exactly as written below. All six panels show the SAME gameplay moment; only the labelled camera variable changes.

STYLE: 2.5D. Real 3D stylized environment, gameplay on a side-scrolling plane, with depth layers in front of and behind the play plane. Fairytale and dreamlike like Trine 4, with Oceanhorn's chunky, Wind Waker-like character proportions. Stylized hand-painted textures, soft global illumination, rich depth fog. Readable at phone size.

SCENE: The first map, an eastern landmark world. A misty mountain path at dusk. A large red-lacquered Japanese torii gate stands on the path and works as a checkpoint; it glows faintly when touched. Stone lanterns, pine trees, distant mountains with low clouds. Mino runs toward the gate, mid-punch against a human soldier in medieval armour holding a torch. A second soldier waits on a ledge above. A toppled nature monument with roots grown through it lies beside the path.

MINO (keep identical in every panel): lean humanoid nature spirit, bark-plated body, two antler-like horns, glowing sap-green chest and joints, short ragged moss cloak, fists only.

PANEL LABELS AND VARIABLES:
1. "1 Reference - full HUD": mobile landscape screenshot. Virtual joystick bottom-left, round attack button bottom-right, pause top-right, mana bar top-left, small checkpoint toast at top center reading "Checkpoint reached". Camera: side-on, slight 10 degree downward tilt.
2. "2 Target - real Japan + refs": no HUD. Same shot, materials grounded in real photos of Fushimi Inari torii, Japanese mountain cedar forests and stone lanterns at dusk.
3. "3 Variant A - camera tilt 25 deg": no HUD. Camera tilted further down to 25 degrees, showing more ground depth.
4. "4 Variant B - camera pulled back 1.5x": no HUD. Camera distance 1.5x, more environment, smaller characters.
5. "5 Variant C - narrow FOV / tele": no HUD. Long-lens look, flattened depth, background mountains feel huge.
6. "6 Variant D - foreground framing": no HUD. Add dark foreground pine branches and grass blades framing the shot, depth of field blur on them.

Do not add any other text, logos or watermarks.
```

## Prompt 3: 3D, "The Valley Breathes"

```
Create ONE wide image: a 3x2 grid concept-art iteration sheet for a mobile action-adventure game called "Natureous". Thin dark gutters between panels. Each panel has a small white label in its top-left corner exactly as written below. All six panels show the SAME valley from the SAME isometric camera; only the labelled variable changes.

STYLE: Full 3D, fixed isometric three-quarter camera like Oceanhorn: blocky stylized terrain with clean detail, chunky Wind Waker-inspired proportions, hand-painted textures, soft sunlight, saturated but natural colours. Readable at phone size.

SCENE: A valley that humans polluted, built around a broken nature monument (a tall standing stone with living roots grown through it). The story: when Mino restores a monument, the map renovates into clean nature with breathable air. Mino stands next to the monument with a grey wolf companion at his side. Swirls of air (white), water (blue) and fire (orange) orbit his hands, his elemental powers unlocked by the monument.

MINO (keep identical in every panel): lean humanoid nature spirit, bark-plated body, two antler-like horns, glowing sap-green chest and joints, short ragged moss cloak.

PANEL LABELS AND VARIABLES:
1. "1 Reference - full HUD": mobile landscape screenshot, valley 0% restored: smog, dead grey grass, smoke from a human camp, broken monument dark. HUD: virtual joystick bottom-left, attack button bottom-right, a second skill button above it with a leaf icon, pause top-right, mana bar top-left, objective chip "Restore the monument".
2. "2 Target - real valley + refs": no HUD. Same 0% valley, materials grounded in real photos of a polluted industrial valley.
3. "3 Restore 25%": no HUD. Smog thinning, first green patches around the monument, monument faintly glowing.
4. "4 Restore 50%": no HUD. Half the valley green, river clearing, camp abandoned.
5. "5 Restore 100%": no HUD. Lush clean valley, flowers, clear blue sky, birds, monument fully glowing, elemental swirls bright.
6. "6 Variant - camera 60 deg top-down": no HUD. The 100% valley from a steeper 60 degree camera to test readability.

Do not add any other text, logos or watermarks.
```

---

## Why these variables

- **2D** tests readability: parallax depth and hero scale. GPGP's artist says leave blank space and keep characters simple but expressive.
- **2.5D** tests the camera, because 2.5D is what the team agreed on and Trine 4's studio returned to 2.5D after full 3D underperformed.
- **3D** tests the story mechanic (map renovation) across one fixed Oceanhorn-style camera, plus one steeper camera to check readability.
- Panel 1 always carries the real HUD at phone size: joystick left and attack button right, matching `MobileInput.cs`.
