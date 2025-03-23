using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;

        [SerializeField] private MenuLevel _level;
        public MenuLevel Level { get { return this._level; } }

        void Awake()
        {
            this._level = MenuLevel.GAMEPLAY;

            MenuManager.Instance = this;
        }

        public void EnterDialogue()
        {
            //add the level as a layer
            this._level |= MenuLevel.DIALOGUE;
        }

        public void ExitDialogue()
        {
            //only should be called after EnterDialogue
            //remove the relevant layer
            this._level ^= MenuLevel.DIALOGUE;
        }
    }
}