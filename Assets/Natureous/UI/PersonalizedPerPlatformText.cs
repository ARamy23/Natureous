using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Natureous
{
    public class PersonalizedPerPlatformText : MonoBehaviour
    {
        static Dictionary<string, string[]> personalizedHintsBasedOnPlatform = new Dictionary<string, string[]> { // index 0 = pc, index 1 = mobile
            { "welcome", new[] {"Welcome to Natureous\n" +
                "Press 'A' or 'D' to move", "Welcome to Natureous\n" +
                "Move the joystick horizontally to walk"} },
            { "jump", new[] { "Press 'Space' to Jump", "Move the joystick vertically to start to jump"} },
            { "sprint", new[] { "Hold 'Left Shift' to sprint while moving", "Move the joystick to its limit horizontally to sprint"} },
            { "climb", new[] { "Press 'Space' and hold the 'W' key to hang onto the ledge then when you are hanging" +
                "\n" +
                "press 'W' and 'Space' to climb it", "Move the joystick diagonally or vertically to climb onto the ledge and keep it in that position to climb it" } },
            { "attack", new[] { "This is an enemy Attack him with 'J' button" +
                "\n" +
                "you can attack while moving, sprinting or while standing" +
                "\n" +
                "BEAT THE CRAB OUTTA HIM!",
                "This is an enemy, Attack him by Tapping on the attack button" +
                "\n" +
                "You can attack while moving, sprinting or while standing" +
                "\n" +
                "BEAT THE CRAB OUTTA HIM!"} },
            { "sprint_jump", new[] { "Press 'Space' when Sprinting (Left shift while moving), go ahead give it a try", "Tilt the joystick diagonally up while sprinting to sprint jump, go ahead give it a try!"} },
            { "running_hammer_down", new[] { "Press 'J' while sprinting and enjoy Thor's hammer! 😂", "Tap 'Attack' while sprinting and enjoy Thor's hammer! 😂" } },
        };

        TextMeshProUGUI textComponent;
        public string key;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            if (key == null || key == "")
                return;
#if UNITY_STANDALONE
            textComponent.text = personalizedHintsBasedOnPlatform[key][0];
#elif UNITY_ANDROID || UNITY_IPHONE
            textComponent.text = personalizedHintsBasedOnPlatform[key][1];
#endif
        }
    }
}


