using OpenTK;
using OpenTK.Input;

namespace SimpleCAD.Source.GUI
{
    public static class InputWrapper
    {
        private static KeyboardState current;

        private static KeyboardState last;
        private static MouseState mouseState;
        private static MouseState prevMouseState;

        private static bool _firstMove = true;
        private static Vector2 mousePos;
        private static Vector2 deltaMouse;

        public static void OnUpdateFrame()
        {
            last = current;
            current = Keyboard.GetState();

            prevMouseState = mouseState;
            mouseState = Mouse.GetState();
            if (_firstMove)
            {
                mousePos = new Vector2(mouseState.X, mouseState.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouseState.X - mousePos.X;
                var deltaY = mouseState.Y - mousePos.Y;

                deltaMouse = new Vector2(deltaX, deltaY);
                mousePos = new Vector2(mouseState.X, mouseState.Y);
            }
        }

        public static Vector2 MouseDeltaPos()
        {
            return deltaMouse;
        }
        public static Vector2 MousePosition()
        {
            return mousePos;
        }

        public static bool KeyDown(Key key)
        {
            return current.IsKeyDown(key);
        }

        public static bool KeyUp(Key key)
        {
            return current.IsKeyUp(key);
        }

        public static bool GetKeyDown(Key key)
        {
            return current.IsKeyDown(key) && last.IsKeyUp(key);
        }

        public static bool GetKeyUp(Key key)
        {
            return current.IsKeyUp(key) && last.IsKeyDown(key);
        }

        public static bool KeyDown(MouseButton key)
        {
            return mouseState[key];
        }

        public static bool KeyUp(MouseButton key)
        {
            return !mouseState[key];
        }

        public static bool GetKeyDown(MouseButton key)
        {
            return !prevMouseState[key] && mouseState[key];
        }

        public static bool GetKeyUp(MouseButton key)
        {
            return prevMouseState[key] && !mouseState[key];
        }

        public static float MouseWheel()
        {
            return mouseState.WheelPrecise - prevMouseState.WheelPrecise;
        }
    }
}
