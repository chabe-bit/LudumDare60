using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameLibrary.Input
{
    public class KeyboardInfo
    {
        // Detect changes in keyboard input between frames 
        public KeyboardState PrevState { get; private set; }

        public KeyboardState CurrState { get; private set; }

        public KeyboardInfo()
        {
            PrevState = new KeyboardState();
            CurrState = Keyboard.GetState();
        }
    

        public void Update()
        {
            PrevState = CurrState;
            CurrState = Keyboard.GetState();
        }

        // The purpose of the IsKeyXXX functions is for the continuous state of a key -> The former is to signal the event that 
        // a key is being held and the other that key is no longer held.
        public bool IsKeyDown(Keys key)
        {
            return CurrState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return CurrState.IsKeyUp(key);
        }

        // The purpose of the WasKeyJustXXX is to detect a single frame state change between key presses -> The former signals a key had just been pressed and
        // the other the key is now released.
        public bool WasKeyJustPressed(Keys key)
        {
            return CurrState.IsKeyDown(key) && PrevState.IsKeyUp(key);
        }

        public bool WasKeyJustReleased(Keys key)
        {
            return CurrState.IsKeyUp(key) && PrevState.IsKeyDown(key);
        }
       



    }
}
