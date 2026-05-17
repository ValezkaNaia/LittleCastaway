using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
  public class StarterAssetsInputs : MonoBehaviour
  {
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    // --- ADICIONADO: Corta a corrida quando ficas sem stamina ---
    private void Update()
    {
        if (SurvivalManager.instance != null && SurvivalManager.instance.isExhausted)
        {
            sprint = false; 
        }
    }

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
    {
      MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
      if(cursorInputForLook)
      {
        LookInput(value.Get<Vector2>());
      }
    }

    public void OnJump(InputValue value)
    {
      JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value)
    {
      SprintInput(value.isPressed);
    }
#endif


    public void MoveInput(Vector2 newMoveDirection)
    {
      move = newMoveDirection;
    } 

    public void LookInput(Vector2 newLookDirection)
    {
      // --- ADICIONADO: Congela a câmara quando estás a ler o Jornal/Notas ---
      if (NoteManager.isReading) 
      {
          look = Vector2.zero;
          return;
      }

      look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
      jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
      sprint = newSprintState;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
      SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
      Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
  }
}