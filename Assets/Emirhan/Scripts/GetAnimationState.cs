using System;
using Unity.VisualScripting;
using UnityEngine;

public class GetAnimationState : MonoBehaviour
{
    public static Action onAnimationFinished;
    
    public void OnAnimationFinished()
    {
        onAnimationFinished?.Invoke();
    }
}
