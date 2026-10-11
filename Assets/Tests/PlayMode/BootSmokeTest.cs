using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BootSmokeTest
{
    // Game code lives in Assembly-CSharp (no asmdef), so look types up by name.
    static Type FindType(string name) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name))
            .FirstOrDefault(t => t != null);

    [UnityTest]
    public IEnumerator Preload_boots_into_Sandbox_with_a_controllable_character()
    {
        SceneManager.LoadScene(0);

        float deadline = Time.realtimeSinceStartup + 20f;
        while (SceneManager.GetActiveScene().name != "Sandbox" && Time.realtimeSinceStartup < deadline)
            yield return null;

        Assert.AreEqual("Sandbox", SceneManager.GetActiveScene().name, "_preload did not load Sandbox within 20 s");

        for (int i = 0; i < 120; i++)
            yield return null;

        var characterType = FindType("Natureous.CharacterControl");
        Assert.IsNotNull(characterType, "Natureous.CharacterControl type not found");

        var characters = UnityEngine.Object.FindObjectsByType(characterType, FindObjectsSortMode.None);
        Assert.IsNotEmpty(characters, "No CharacterControl in Sandbox after boot");
        Debug.Log($"[smoke] Sandbox booted with {characters.Length} characters");
    }
}
