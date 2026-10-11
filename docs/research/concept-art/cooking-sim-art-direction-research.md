# Research: Cooking-Sim Art Direction (Papa's ...ria series and Good Pizza, Great Pizza) for Concept-Iteration Sheets

**Date**: 2026-10-11 | **Researcher**: nw-researcher (Nova) | **Confidence**: Medium (production facts) / Low (visual-layout specifics) | **Sources**: 11 cited

## Executive Summary

Both games are **flat 2D, station-based** cooking sims. In each one the camera cuts between fixed single-purpose screens instead of moving through a space. The Papa's series (Flipline Studios, Cleveland, from 2007) began as a Flash browser game. Its ports to touch devices (the "To Go!" versions, from 2012) put station switching on corner thumb buttons and added a zoomed-in mode for order tickets. Good Pizza, Great Pizza (TapBlaze, iOS 2014) has one sole artist, Weiling Peng. Her stated rules are "simple but expressive" characters, "warm, earthy-toned" appetizing colours, toppings drawn true to life so players spot them fast, and leaving blank space on purpose. TapBlaze's visual identity improved through **small, step-by-step polish updates to design and UI**. Those updates followed a 2016 app-store-editor critique, and daily active users went from about 1k to 30k in 3-4 months (GDC 2018 talk by Anthony Lai).

I found **no** first-party art postmortem, GDC art talk or interview about how Flipline's art direction was developed. The Flipline blog blocks automated access. All statements below about Papa's camera framing, palette and HUD placement come from my own description of the games and are marked **[unverified-observational]**. Check them against screenshots before relying on them.

The rules you can carry over (last section) come down to four:
- Each panel shows **one station or one camera**.
- Each variant changes **one variable**.
- Readability comes first: silhouette, true-to-life props, blank space.
- Iterate in **small steps against outside critique**. Don't redesign everything at once.

## Research Methodology

**Search strategy**: web search plus direct fetches of Wikipedia, GDC Vault, Steam, the Apple App Store editorial "Behind the Scenes" story, the Google AdMob case study, a Pocket Tactics artist interview, and TouchArcade listings.
**Source selection**: the trusted list in `.nwave/trusted-source-domains.yaml` (its game_industry block). Sources outside that list (pockettactics.com, google.* ads stories, gdconf.com) are marked "non-listed" and treated as medium. They are cross-referenced where possible.
**Blocked**: flipline.com/blog, i.flipline.com, tapblaze.com and toucharcade.com all returned 403. flipline.fandom.com returned 402.

## Findings

### A. Papa's ...ria series (Flipline Studios)

#### A1. Studio and lineage
**Evidence**: Founded in Cleveland, Ohio by Matt Neff and Tony Solary in 2004. Papa's Pizzeria was released in 2007 as a Flash browser game. Mobile "To Go!" versions have existed since 2012, "HD" tablet versions ran 2012-2019, and Steam "Deluxe" remakes since 2023.
**Source**: [Wikipedia: Papa Louie](https://en.wikipedia.org/wiki/Papa_Louie). Accessed 2026-10-11. Medium-high.
**Verification**: Steam lists Papa's Pizzeria Deluxe as released 2024-11-19 by Flipline ([Steam 3259470](https://store.steampowered.com/app/3259470/)). Search snippets (not fetched directly) say Solary studied at the Cleveland Institute of Art, which would make him an illustrator-founder.
**Confidence**: Medium.
**Analysis**: Because the series started in Flash, its art is almost certainly vector-based: clean outlines, flat fills, scaling without loss. I could not find Flipline confirming the toolchain, so this is **[inference]**.

#### A2. Core loop: four discrete stations
**Evidence**: The games "typically involve four stations: order-taking, food preparation (split across two stations), and plating/serving". In Pizzeria these are **Order, Build/Topping, Bake, Cut**. Bake has Light/Regular/Well Done targets. Toppings can be placed by dragging from a bin, or by clicking a bin and then clicking the pizza.
**Source**: [Wikipedia](https://en.wikipedia.org/wiki/Papa_Louie). Station detail is from search results describing Deluxe (Flipline blog [?p=19577](https://www.flipline.com/blog/?p=19577), which was blocked, so this is snippet-level only).
**Verification**: The Steam Freezeria Deluxe page says "each station is a unique hands-on experience" and offers "Score Details to learn from your mistakes" ([Steam](https://store.steampowered.com/app/2291760/)). The Steam Pizzeria Deluxe page lists "151 customers".
**Confidence**: Medium-High.

#### A3. Mobile adaptation (To Go!)
**Evidence**: Players "switch stations using buttons in the corners of the screen, perfect for your thumbs". Buttons in the top corners switch Order Tickets, and all tickets can be viewed "in a zoomed-in mode for easy reading". Stations are played by "drag, swipe, and tap".
**Source**: [TouchArcade listing](https://toucharcade.com/games/papas-pizzeria-to-go). Accessed 2026-10-11 through a search snippet; a direct fetch returned 403. Medium-high domain.
**Confidence**: Medium (one source, store-copy wording).
**Analysis**: This is the clearest documented mobile-readability decision. They **did not shrink the desktop HUD**. Instead they moved navigation to the thumb zones and gave the order ticket its own enlarged view.

#### A4. Camera, HUD and palette per station **[unverified-observational]**
- **Order**: a side-on, eye-level view of the counter with customers in a queue.
- **Build/Topping**: a near top-down view of the pizza, with ingredient bins in a row along the screen edge.
- **Bake**: a front-on oven with timer slots.
- **Cut**: a top-down view of the pizza with guide lines.
- **Order tickets**: hang on a rail at the top of the screen and can be dragged between stations.
- **Grading**: a per-station percentage score adds up to a tip and a star meter at the end of each order.
- **Visual style**: thick dark outlines, flat cel shading, saturated warm colours, and chibi-proportioned customers with oversized heads.

No trusted source documents these layouts. Treat them as hypotheses and check them against official screenshots (Steam or the App Store) before using them in panel 1 of a sheet.

### B. Good Pizza, Great Pizza (TapBlaze)

#### B1. Facts and loop
**Evidence**: Released on iOS 2014-12-02, followed by Android, Switch (2020) and PC (2023). Players fulfil customer orders, which can be ambiguous, and compete with a rival shop. The steps are dough, sauce, toppings, bake, slice and box. Money buys ingredients and upgrades. Wrong orders lead to refunds. There are more than 100 customers. The game won NYX 2024 Gold for PC Visual Art and for Character Design.
**Source**: [Wikipedia: GPGP](https://en.wikipedia.org/wiki/Good_Pizza,_Great_Pizza). Accessed 2026-10-11. Medium-high.
**Confidence**: Medium-High.
**Analysis**: The core difficulty is **interpreting the order**, not keeping up with a timer. That puts the HUD weight on the customer's face and speech bubble, not on a ticket rail.

#### B2. Art direction, in the artist's own words
**Evidence**:
- "We try to keep designs simple but expressive - just enough detail to show personality and identity without overwhelming the screen."
- "It's okay to leave some blank space - what you don't draw can be just as powerful."
- "a warm, earthy-toned palette that feels cozy and appetizing." The sister coffee game uses adjusted tones for "a calm morning vibe".
- Characters are drawn from real people. Toppings start "from real life … how they're cut, and how they sit on a pizza" and must stay "recognizable, appetizing, and true to life".
- Cosmetic topping skins can take any shape but must remain "readable".

**Source**: [Pocket Tactics interview with Weiling Peng](https://www.pockettactics.com/good-pizza-great-pizza-interview). Accessed 2026-10-11. Non-listed (medium).
**Verification**: TapBlaze's sole artist "worked in a pizza shop for 4 years" ([Google AdMob case study](https://www.google.cz/ads/publisher/stories/tapblaze/), non-listed). The Apple editorial says the shop's brick wall and upholstery copy a real neighbourhood pizzeria, and that an Edward Hopper painting was placed in the scene ([App Store story](https://apps.apple.com/ca/story/id1446242882), medium-high).
**Confidence**: Medium. All three sources agree on the "grounded in real life" principle.

#### B3. How the identity was iterated
**Evidence**: At GDC 2016 an app-store editor said TapBlaze had not polished the game's "design, UI, and monetization". The team then made "small, incremental updates" and DAU went from about 1k to 30k in 3-4 months. Over a year, DAU grew from 3k to more than 60k organically. Separately, it took "another 2 years of improvements" and a reworked opening narrative to fix retention.
**Source**: [GDC Vault 1025561, Anthony Lai, 2018](https://gdcvault.com/play/1025561/-Good-Pizza-Great-Pizza), medium-high. [gdconf.com news](https://gdconf.com/news/get-mobile-game-dev-tips-maker-good-pizza-great-pizza-gdc-2018) (from a search snippet; a direct fetch returned 404). [Google AdMob story](https://www.google.cz/ads/publisher/stories/tapblaze/).
**Verification**: Wikipedia also reports the growth from 1,000 to 30,000 DAU.
**Confidence**: High on the process. The talk video itself was not watched, so I have no specific detail on which art changes were made.

### C. Mobile readability: synthesis
- **Navigation**: Papa's moved navigation to corner thumb buttons and gave tickets a zoom view (A3).
- **Restraint**: GPGP holds back on detail, leaves blank space, and keeps food true to life so players identify it instantly (B2).
- **Warm palette**: GPGP's palette is warm and appetizing (B2). For Papa's this is only observed, not documented (A4).
- **One task per screen**: both games use one fixed camera per task (A2 and B1).
- **Polish matters**: in GPGP's case, UI polish had a measurable effect on growth (B3).

## Source Analysis
| Source | Domain | Reputation | Access | Cross-verified |
|---|---|---|---|---|
| Wikipedia: Papa Louie | en.wikipedia.org | Medium-high | 2026-10-11 | Y (Steam) |
| Wikipedia: GPGP | en.wikipedia.org | Medium-high | 2026-10-11 | Y (GDC) |
| GDC Vault 1025561 | gdcvault.com | Medium-high | 2026-10-11 | Y |
| Steam: Pizzeria Deluxe / Freezeria Deluxe | store.steampowered.com | Medium-high | 2026-10-11 | Y |
| App Store story: Behind the Scenes | apps.apple.com | Medium-high | 2026-10-11 | Partial |
| TouchArcade: Papa's Pizzeria To Go (snippet) | toucharcade.com | Medium-high | 2026-10-11 | N |
| Flipline blog ?p=19577 (snippet only) | flipline.com | Medium-high | 2026-10-11 | N |
| Pocket Tactics: Weiling Peng interview | pockettactics.com | Non-listed (medium) | 2026-10-11 | Partial |
| Google AdMob: TapBlaze story | google.* | Non-listed (medium) | 2026-10-11 | Partial |
| gdconf.com news (snippet) | gdconf.com | Non-listed (medium) | 2026-10-11 | Y (GDC Vault) |

No excluded domains were used.

## Knowledge Gaps
1. **Flipline art-direction history**: I found no interview, postmortem or GDC talk. flipline.com returned 403 and the fandom wiki returned 402. *Next step*: browse the Flipline blog "sneak peek" posts manually, since they often show redrawn art before release.
2. **Exact camera and HUD layouts for both games**: no documented source exists. *Next step*: capture screenshots from the Steam and App Store galleries and annotate them.
3. **GPGP art toolchain** (vector or raster, rig or frame animation): I did not watch the GDC 2018 video, and the TapBlaze job pages returned 403.
4. **Whether the Papa's HD and Deluxe versions were redrawn or upscaled**: unverified.

## Conflicting Information
There are no direct conflicts. One small discrepancy: the DAU baseline is "3,000 to 60,000 in a year" in one place and "1,000 to 30,000 in 3-4 months" in another. These are probably different time windows within the same growth story.

## Transferable rules for concept-iteration sheets (Natureous)

- **One gameplay context per panel, with a fixed camera.** Both games split play into single-purpose screens (A2, B1). For Natureous, choose one moment per sheet, such as exploration, combat or a puzzle room, and lock everything except the variable being tested.
- **Panel 1 = reference with the full, real HUD at device resolution.** Papa's handled mobile by moving and zooming the HUD rather than shrinking it (A3). Concept frames without a HUD lie about how readable the game is. Draw the thumb zones on the frame, especially for 2.5D or 3D Natureous on phone.
- **Panel 2 = target grounded in real-world references.** GPGP's art comes from a real pizzeria, real people and real toppings (B2). Attach photos of the real coastal, ruin and flora references next to the Oceanhorn-style target.
- **Panels 3-6 change one variable each:** camera height, FOV, prop scale, character or food scale. The scale variants test Peng's "recognizable … true to life" and "simple but expressive" criteria (B2). The camera variants test which view makes the interaction clearest, the same way the games use top-down for building and side-on for customers (A4, unverified).
- **Score every panel on readability:** can the player pick out the player character, interactables and threats at a glance and at phone size? Apply "leave blank space" (B2) and the "silhouette > detail" interpretation.
- **Choose the palette by emotional target, then vary only the tone across sub-themes.** GPGP moved its palette from "cozy, appetizing" to "calm morning" for the coffee game while keeping the system the same (B2). Do the same for each Natureous biome.
- **Iterate in small, external-critique-driven steps.** GPGP's identity improved through incremental polish after an outside review, not a full redesign (B3). Run each sheet round past one outside reviewer and change no more than one or two variables per round.
- **Render dimension variants (2D, 2.5D, 3D) as separate sheets, not separate panels.** This is an interpretation: mixing dimension with camera variables breaks the one-variable rule that makes the sheets comparable.

## Full Citations
[1] Wikipedia. "Papa Louie". https://en.wikipedia.org/wiki/Papa_Louie. Accessed 2026-10-11.
[2] Wikipedia. "Good Pizza, Great Pizza". https://en.wikipedia.org/wiki/Good_Pizza,_Great_Pizza. Accessed 2026-10-11.
[3] Lai, A. "'Good Pizza, Great Pizza': Game Design, Iteration, and Business Lessons Learned". GDC 2018. https://gdcvault.com/play/1025561/-Good-Pizza-Great-Pizza. Accessed 2026-10-11.
[4] GDC. News item (snippet). https://gdconf.com/news/get-mobile-game-dev-tips-maker-good-pizza-great-pizza-gdc-2018. Accessed 2026-10-11.
[5] Pocket Tactics. "Good Pizza, Great Pizza's artist talks toppings…". https://www.pockettactics.com/good-pizza-great-pizza-interview. Accessed 2026-10-11.
[6] Apple App Store. "Behind the Scenes". https://apps.apple.com/ca/story/id1446242882. Accessed 2026-10-11.
[7] Google AdMob. "TapBlaze" story. https://www.google.cz/ads/publisher/stories/tapblaze/. Accessed 2026-10-11.
[8] Steam. "Papa's Pizzeria Deluxe". https://store.steampowered.com/app/3259470/. Accessed 2026-10-11.
[9] Steam. "Papa's Freezeria Deluxe". https://store.steampowered.com/app/2291760/. Accessed 2026-10-11.
[10] TouchArcade. "Papa's Pizzeria To Go!" (snippet). https://toucharcade.com/games/papas-pizzeria-to-go. Accessed 2026-10-11.
[11] Flipline Blog (snippet). https://www.flipline.com/blog/?p=19577. Accessed 2026-10-11.
