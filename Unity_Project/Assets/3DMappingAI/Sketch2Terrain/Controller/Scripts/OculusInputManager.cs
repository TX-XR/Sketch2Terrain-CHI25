using UnityEngine.XR;
using MappingAI;
using UnityEngine;

//[CreateAssetMenu(fileName = "OculusInputManager", menuName = "3DMappingAISettings", order = 2)]
public static class OculusInputManager
{
    public static OVRInput.Controller GetDominatehand()
    {
        return (ApplicationSettings.Instance.primaryHand == XRNode.RightHand) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
    }

    public static bool GetRightControllerState()
    {
        return OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
    }
    public static bool GetLeftControllerState()
    {
        return OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
    }
    public static OVRInput.Controller GetUndominatehand()
    {
        return (ApplicationSettings.Instance.primaryHand == XRNode.RightHand) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
    }

    public static bool IsLoadAnchorsPrimaryIndexTrigger(bool isDominatehand = false)
    {   if (isDominatehand)
            return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, GetDominatehand()) > 0.2;
        else
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, GetUndominatehand());
    }
    public static bool IsStylusTouch()
    {
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        return OVRInput.Get(OVRInput.Axis1D.PrimaryStylusForce, GetDominatehand()) > 0.1f;
    }
    public static bool CanSketchOrErase()
    {
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetUndominatehand()) || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.3f;
        // if undominate hand's primary hand trigger is down, the zoom is hot
        return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominatehand()) > 0.2f;
        //if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetUndominatehand()) > 0.2f)
        //    return false;
        //else
            
    }
    public static float GetSketchPresure()
    {
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return 0;
        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, GetUndominatehand()))
            return 0;
        else
            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominatehand());
    }

    public static bool SwitchSystemDown()
    {
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        return OVRInput.GetDown(OVRInput.Button.Two, GetDominatehand());
    }
    public static bool NextStudy()
    {
        //if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
        //    return false;
        //else
        //    return OVRInput.GetDown(OVRInput.Button.Two, GetUndominatehand());

        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        return OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, GetUndominatehand());
    }

    public static bool BackToTaskKey()
    {
        //return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.1f;
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetDominateHand());
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, GetUndominatehand());
    }
    /// <summary>
    /// in SurfaceCalibration mode, Button two means calibrate
    /// in Sketch mode, Button two means Undo
    /// </summary>
    /// <returns></returns>
    private static bool ButtonOneDown_Undominatehand()
    {
        return OVRInput.GetDown(OVRInput.Button.One, GetUndominatehand());
    }
    public static bool ButtonOneDown(bool isDominateHand)
    {
        if (isDominateHand)
            return ButtonOneDown_Dominatehand();
        else
            return ButtonOneDown_Undominatehand();
    }
    private static bool ButtonOneDown_Dominatehand()
    {
        return OVRInput.GetDown(OVRInput.Button.One, GetDominatehand());
    }

    public static bool ButtonTwoDown(bool isDominateHand)
    {
        if (isDominateHand)
            return ButtonTwoDown_Dominatehand();
        else
            return ButtonTwoDown_Undominatehand();
    }
    /// <summary>
    /// in SurfaceCalibration mode, Button two means reset
    /// in Sketch mode, Button two means Redo
    /// </summary>
    /// <returns></returns>
    private static bool ButtonTwoDown_Undominatehand()
    {
        return OVRInput.GetDown(OVRInput.Button.Two, GetUndominatehand());
    }

    private static bool ButtonTwoDown_Dominatehand()
    {
        return OVRInput.GetDown(OVRInput.Button.Two, GetDominatehand());
    }
    public static bool GripButtonDown_Undominatehand()
    {
        //return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.1f;
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetDominateHand());
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, GetUndominatehand()))
        {
            return true;
        }
        return false;
    }
    public static bool GripButtonUp_Undominatehand()
    {
        //return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.1f;
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetDominateHand());
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, GetUndominatehand()))
        {
            return true;
        }
        return false;
    }
    public static bool zoomActionUp()
    {
        //return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.1f;
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetDominateHand());
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;

        if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, GetDominatehand()))
        {
            return true;
        }
        return false;
    }
    public static bool zoomActionDown()
    {
        //return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.1f;
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetDominateHand());
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetUndominatehand()) > 0.2f)
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominatehand()) > 0.2f)
            {
                return true;
            }
        }
        return false;
    }

    public static bool triggerButtonDown_Undominatehand()
    {
        //return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.1f;
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetDominateHand());
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, GetUndominatehand());
    }


    public static bool triggerButtonUp_Undominatehand()
    {
        //return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetDominateHand()) > 0.1f;
        //return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, GetDominateHand());
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, GetUndominatehand());
    }


    // trigger the state of grid, hide or show
    public static bool gridStateDown()
    {
        if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            return false;
        return OVRInput.GetDown(OVRInput.Button.One, GetDominatehand());
    }

    public static void SetVibration(bool isDominateHand = true)
    {
        if (isDominateHand)
        {
            OVRInput.SetControllerLocalizedVibration(OVRInput.HapticsLocation.Hand, 0f, 0.5f, GetDominatehand());
        }
        else
        {
            OVRInput.SetControllerLocalizedVibration(OVRInput.HapticsLocation.Hand, 0f, 0.5f, GetUndominatehand());
        }
    }
    //public static bool ChangeColor()
    //{
    //    return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetUndominatehand()) > 0.8f;
    //}
}
