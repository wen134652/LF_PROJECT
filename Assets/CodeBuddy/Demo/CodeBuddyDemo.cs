using UnityEngine;

namespace CodeBuddy.Demo
{
    public class CodeBuddyDemo : MonoBehaviour
    {
        public void JoinDiscord()
        {
            Application.OpenURL("https://discord.gg/JdsepFhEeX");
        }

        public void OpenDocumentation()
        {
            Application.OpenURL("https://docs.driftingmoose.com/");
        }
    }
}
