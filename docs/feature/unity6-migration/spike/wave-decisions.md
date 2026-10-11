# Wave decisions: unity6-migration spike

- **Phase A (static probe):** done. Verdict: LIKELY OPENS. No removed APIs in own code. The risks are concentrated in URP 7→17, Cinemachine 2.5, and third-party SDKs (AppCenter, GameAnalytics `WWW`, PlayServicesResolver, legacy Android `.aar` files).
- **Direct upgrade (no 2022 LTS hop):** proposed. Revisit if the URP asset upgrade fails in Phase B.
- **Promotion decision:** PENDING owner: phase B verdict WORKS (see findings). Options: PROMOTE (merge this branch into develop), or redo cleanly on develop.
- **Caveat:** Unity 6 statuses in findings.md are UNVERIFIED because there was no web access during the probe.
