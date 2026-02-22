using System;
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
		public bool shoot;
		public bool zoom;
		public float scrollValue;
		public Action<int> slotActionByKeyboard;
		public Action<float> slotActionByScroll;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

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

		public void OnShoot(InputValue value)
		{
			ShootInput(value.isPressed);
		}

		public void OnEquipGunKeyboard(InputValue value)
		{
			if (value.isPressed)
			{
				// hangi tuş basıldı?
				var control = Keyboard.current;
				if (control.digit1Key.wasPressedThisFrame)
					slotActionByKeyboard?.Invoke(1);
				else if (control.digit2Key.wasPressedThisFrame)
					slotActionByKeyboard?.Invoke(2);
				else if (control.digit3Key.wasPressedThisFrame)
					slotActionByKeyboard?.Invoke(3);
			}
		}

		public void OnZoom(InputValue value)
		{
			ZoomInput(value.isPressed);
		}

		public void OnEquipGunScroll(InputValue value)
		{
			slotActionByScroll?.Invoke(value.Get<float>());
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
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

		public void ShootInput(bool newShootState)
		{
			shoot = newShootState;
		}
		
		public void ZoomInput(bool newZoomState)
		{
			zoom = newZoomState;
		}
		
		public void ScrollInput(float scroll)
		{
			scrollValue = scroll;
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