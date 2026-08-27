using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager current;

    public InputActionReference CatMove;
    public InputActionReference CatTurn;
    //public InputActionReference TurnUpDown;
    public InputActionReference CatJump;
    public InputActionReference CatAttack;

    public InputActionReference RatMove;
    public InputActionReference RatTurn;
    public InputActionReference RatJump;
    public InputActionReference RatPickUp;
    public InputActionReference RatBlast;

    void Awake()
    {
        if (current)
        {
            Destroy(gameObject);
        }
        else
        {
            current = this;

            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        CatMove.action.Enable();
        CatJump.action.Enable();
        CatTurn.action.Enable();
        CatAttack.action.Enable();

        RatMove.action.Enable();
        RatJump.action.Enable();
        RatTurn.action.Enable();
        RatPickUp.action.Enable();
        RatBlast.action.Enable();
    }

    public void DisableCat()
    {
        CatMove.action.Disable();
        CatJump.action.Disable();
        CatTurn.action.Disable();
        CatAttack.action.Disable();

    }

    public void EnableCat()
    {
        CatMove.action.Enable();
        CatJump.action.Enable();
        CatTurn.action.Enable();
        CatAttack.action.Enable();
    }

    public void DisableRat()
    {
        RatMove.action.Disable();
        RatJump.action.Disable();
        RatTurn.action.Disable();
        RatPickUp.action.Disable();
        RatBlast.action.Disable();
    }

    public void EnableRat()
    {
        RatMove.action.Enable();
        RatJump.action.Enable();
        RatTurn.action.Enable();
        RatPickUp.action.Enable();
        RatBlast.action.Enable();

        //fixes turn
        RatTurn.action.actionMap.asset.devices = null;
    }
  
}
