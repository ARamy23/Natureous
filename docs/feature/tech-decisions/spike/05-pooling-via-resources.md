# 05 — Pooling through Resources by enum name

## Decision
- `enum PoolObjectType { AttackInfo, ThorHammer, HammerDownVFX }`. `PoolObjectLoader.InstantiatePrefab` calls `Resources.Load(objectType.ToString())` and then `Instantiate` (`PooledObjects/PoolObjectLoader.cs:7-21`).
- `PoolManager : Singleton` keeps a `Dictionary<PoolObjectType, List<GameObject>>`. Objects come from the front of the list (`list[0]` plus `RemoveAt(0)`), or a new one is instantiated when the list is empty. There is no pre-warm and no cap (`Managers/PoolManager.cs:25-47`). Return is `SetActive(false)` plus add to the list (`:49-54`).
- `PoolObject` handles timed auto-return through a coroutine. That coroutine checks `List.Contains` to avoid returning an object twice (`PooledObjects/PoolObject.cs:15-49`).
- Callers: `Attack.OnEnter` (`States/.../Attack.cs:27`), `SpawnObject` (hammer and VFX), and `Attack.DeregisterAttack` calling `TurnOff` (`Attack.cs:105`).
- Similar `Resources.Load`-by-string calls exist outside the pool: `DeathAnimationsManager.cs:16` and `SendPathFindingAgent.cs:17`.

## Goal behind it
- **Inferred:** avoid instantiate and destroy cost, and garbage, during combat (`where-we-wanted-to-be.md` §2 says "The original reason isn't recorded anywhere").
- **Stated (commits):** "Spawn hammer when doing special attack" (`85f1605`) and "Add VFX and attack effects to the hammer down" (`72bace1`) introduce the pooled hammer and VFX. "improve attack system" (`476b4d2`) is undocumented.

## What it achieved / where it hurt
- **Achieved:** attacks and VFX are reused once the first instance exists.
- **Hurt: lazy creation.** There is no pre-warm, so the first use of each type calls `Resources.Load` and `Instantiate` in the middle of combat. Whether that causes a hitch is unverified.
- **Hurt: lost return.** `GetObject` can return null if the asset name does not match the enum. In that case `Attack.cs:28` throws a NullReferenceException. `where-we-wanted-to-be.md` §2 claims "fallback is instantiate-on-demand", but that is the empty-pool path, not the missing-asset path. Pooled objects also become children of characters (`PoolObject.TurnOff` resets `parent`), and if the owner is destroyed while an object is active, that object is never returned.
- **Hurt: list costs.** `RemoveAt(0)` and `Contains` are linear list operations, which is small at current pool sizes.
- **Hurt: build size.** All `Resources/` content ships in the build and is indexed at startup. That includes 6 project folders, one of them in `Roundbeargames_Tutorial`.
- **Hurt: type and name coupling.** Adding a new pooled type means editing the enum and naming the prefab to match exactly.

## Unity 6 status
`Resources.Load` is still supported, but Unity's docs recommend Addressables for new content (UNVERIFIED; not covered in the migration findings). `Object.Instantiate` is unchanged. Unity has shipped `UnityEngine.Pool.ObjectPool<T>` since 2021.1 (UNVERIFIED; not in the migration findings).

## Probe question
Does the first `PoolManager.GetObject(PoolObjectType.ThorHammer)` call during combat produce a measurable frame spike compared with later calls?

**Probe method:** a PlayMode test that wraps `GetObject` in a `ProfilerMarker` and calls it for each type twice: once cold, then once after `TurnOff`. It records the marker time and `GC.Alloc` with `ProfilerRecorder`, run in a development player build (`-batchmode -buildTarget` plus `-runTests` on the player). An EditMode test also asserts that every `PoolObjectType` name resolves to a `Resources` prefab carrying a `PoolObject`.
**Budget:** under 3 h.

## Pending research
- Should the custom pool be replaced by `UnityEngine.Pool.ObjectPool<T>`, which has create, get, release and destroy callbacks, a max size and a collection check?
- Addressables versus serialized prefab references on a pool config ScriptableObject: which removes the name/enum coupling with less tooling?
- Should pools pre-warm during `_preload` or on level load?
- Does `AttackInfo` need to be a GameObject at all, or could it be a plain C# object or struct pooled in memory (see decision 06)?

## Verdict
**REVISIT** (provisional). Pooling itself is a reasonable fit. The Resources-by-enum loading and the hand-rolled pool are the parts to re-examine.
