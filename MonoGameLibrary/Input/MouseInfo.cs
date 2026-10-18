using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary.Input
{
    public class MouseInfo
    {
        public MouseState PrevState { get; private set; }
        public MouseState CurrState { get; private set; }


        // Properties to handle cursor position
        public Point Position
        {
            get => CurrState.Position;
            set => SetPosition(value.X, value.Y);
        }

        // What is this "value" keyword C# ?
        // I understand it must a run-time thing where any type of value 
        // can be passed in, but this is just lazy. But lazy might be better.
        public int X
        { 
            get => CurrState.X;
            set => SetPosition(value, CurrState.Y); 
        }

        public int Y
        {
            get => CurrState.Y;
            set => SetPosition(CurrState.X, value);
        }

        // Track cursor pos 
        public Point PosDelta => CurrState.Position - PrevState.Position;
        
        // Track X movement
        public int XDelta => CurrState.X - PrevState.X;
    
        // Track Y movement
        public int YDelta => CurrState.Y - PrevState.Y;

        public bool WasMoved => PosDelta != Point.Zero;

        public int ScrollWheel => CurrState.ScrollWheelValue;

        public int ScrollWheelData => CurrState.ScrollWheelValue - PrevState.ScrollWheelValue;

        public MouseInfo()
        {
            PrevState = new MouseState();
            CurrState = Mouse.GetState();
        }

        public bool IsButtonDown(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return CurrState.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return CurrState.RightButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return CurrState.MiddleButton == ButtonState.Pressed;
                case MouseButton.XButton1:
                    return CurrState.XButton1 == ButtonState.Pressed;
                case MouseButton.XButton2:
                    return CurrState.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public bool IsButtonUp(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return CurrState.LeftButton == ButtonState.Released;
                case MouseButton.Right:
                    return CurrState.RightButton == ButtonState.Released;
                case MouseButton.Middle:
                    return CurrState.MiddleButton == ButtonState.Released;
                case MouseButton.XButton1:
                    return CurrState.XButton1 == ButtonState.Released;
                case MouseButton.XButton2:
                    return CurrState.XButton2 == ButtonState.Released;
                default:
                    return false;
            }
        }

        public bool WasButtonJustPressed(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return  CurrState.LeftButton    == ButtonState.Pressed && 
                            PrevState.LeftButton    == ButtonState.Released;
                case MouseButton.Right:
                    return  CurrState.RightButton   == ButtonState.Pressed && 
                            PrevState.RightButton   == ButtonState.Released;
                case MouseButton.Middle:
                    return  CurrState.MiddleButton  == ButtonState.Pressed && 
                            PrevState.MiddleButton  == ButtonState.Released;
                case MouseButton.XButton1:
                    return  CurrState.XButton1      == ButtonState.Pressed && 
                            PrevState.XButton1      == ButtonState.Released;
                case MouseButton.XButton2:
                    return  CurrState.XButton2      == ButtonState.Pressed && 
                            PrevState.XButton2      == ButtonState.Released;
                default:
                    return false;
            }
        }

        public bool WasButtonJustReleased(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return  CurrState.LeftButton    == ButtonState.Released &&
                            PrevState.LeftButton    == ButtonState.Pressed;
                case MouseButton.Right:
                    return  CurrState.RightButton   == ButtonState.Released &&
                            PrevState.RightButton   == ButtonState.Pressed;
                case MouseButton.Middle:
                    return  CurrState.MiddleButton  == ButtonState.Released &&
                            PrevState.MiddleButton  == ButtonState.Pressed;
                case MouseButton.XButton1:
                    return  CurrState.XButton1      == ButtonState.Released &&
                            PrevState.XButton1      == ButtonState.Pressed;
                case MouseButton.XButton2:
                    return  CurrState.XButton2      == ButtonState.Released &&
                            PrevState.XButton2      == ButtonState.Pressed;
                default:
                    return false;
            }
        }


        public void SetPosition(int x, int y)
        {
            Mouse.SetPosition(x, y);
            CurrState = new MouseState(
                x,
                y,
                CurrState.ScrollWheelValue,
                CurrState.LeftButton,
                CurrState.MiddleButton,
                CurrState.RightButton,
                CurrState.XButton1,
                CurrState.XButton2
            );
        }


    }
}
