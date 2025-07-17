using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ErrorEventHandler
{
    public delegate void HitAnErrorHandler(string header, string body, Color? headerColor);
    public static event HitAnErrorHandler onHitAnError;
    public static void InvokeError(string header="Oops!", string body="An error has occurred", Color? headerColor = null)
    {
        //InvokeError is called by objects that hit an error 
        onHitAnError?.Invoke(header, body, headerColor);
    }

    //ErrorEventHandler.InvokeError("error!", "error details", Color.red);
    //ErrorEventHandler.onHitAnError += DisplayError;
}
