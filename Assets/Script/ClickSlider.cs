using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class xUISliderPointerEvent : UnityEvent
{ }

public class ClickSlider : Slider
    , IPointerDownHandler
    , IPointerUpHandler
{

    public xUISliderPointerEvent mOnPointerDown = new xUISliderPointerEvent();
    public xUISliderPointerEvent mOnPointerUp = new xUISliderPointerEvent();

    public  override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (this.mOnPointerDown != null)
            mOnPointerDown.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (this.mOnPointerUp != null)
            mOnPointerUp.Invoke();
    }

    public void SetValue(float value, bool isNoti)
    {
        this.Set(value, isNoti);
    }
}//end class