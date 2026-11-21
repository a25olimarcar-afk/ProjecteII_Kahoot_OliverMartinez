using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class VirtualGamepadTester : MonoBehaviour
{
    private Gamepad virtualGamepad;

    private void OnEnable()
    {
        //creamos un mando virtual para realizar las pruebas
        virtualGamepad = InputSystem.AddDevice<Gamepad>();

        Debug.Log("Virtual Gamepad created.");
    }

    private void Update()
    {
        if (virtualGamepad == null || Keyboard.current == null)
        { return; }

        //creamos un estado nuevo para el mando
        GamepadState gamepadState = new GamepadState();

        //simulamos la cruceta con las teclas I, K, J y L
        gamepadState = gamepadState.WithButton(
            GamepadButton.DpadUp, Keyboard.current.iKey.isPressed);

        gamepadState = gamepadState.WithButton(
            GamepadButton.DpadDown, Keyboard.current.kKey.isPressed);

        gamepadState = gamepadState.WithButton(
            GamepadButton.DpadLeft, Keyboard.current.jKey.isPressed);

        gamepadState = gamepadState.WithButton(
            GamepadButton.DpadRight, Keyboard.current.lKey.isPressed);

        //simulamos los botones principales del mando
        gamepadState = gamepadState.WithButton(
            GamepadButton.South, Keyboard.current.uKey.isPressed);

        gamepadState = gamepadState.WithButton(
            GamepadButton.East, Keyboard.current.oKey.isPressed);

        //enviamos el estado al mando virtual
        InputSystem.QueueStateEvent(virtualGamepad, gamepadState);
    }

    private void OnDisable()
    {
        //eliminamos el mando virtual al terminar las pruebas
        if (virtualGamepad != null && virtualGamepad.added)
        {
            InputSystem.RemoveDevice(virtualGamepad);
        }
    }
}
