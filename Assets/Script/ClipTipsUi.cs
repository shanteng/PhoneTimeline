using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

using UnityEngine.Timeline;
using System;
using System.Reflection;


//add by tiny 一个可以让游戏暂停恢复、逐帧执行的脚本
public class ClipTipsUi : MonoBehaviour
{
    public ClickSlider _StartSlider;
    public ClickSlider _DurationSlider;

    public Text _StartTxt;
    public Text _DuraTxt;

    public ScrollRect _scrollInspector;
    public PropertyItem _Templete;
    private List<PropertyItem> _childItems = new List<PropertyItem>();

    private TimelineClip _clip;
    private string _uid;
    private float _totleDuratation;

    public static ClipTipsUi Ins;
    private void Awake()
    {
        this._Templete.gameObject.SetActive(false);
        Ins = this;
    }


    public void OnDuraChange(float addValue)
    {
        float lastValue = this._DurationSlider.value + addValue;
        if (lastValue < this._DurationSlider.minValue)
            this._DurationSlider.value = this._DurationSlider.minValue;
        else
            this._DurationSlider.value = lastValue;
    }

    public void OnStartValueChange(float addValue)
    {
        float lastValue = this._StartSlider.value + addValue;
        if (lastValue < 0)
            lastValue = 0;
        this._StartSlider.value = lastValue;
    }

    private void OnValueChanged(float value)
    {
        if (this._StartSlider.value + this._DurationSlider.value > _totleDuratation)
            this._StartSlider.SetValue(_totleDuratation - this._DurationSlider.value,false);
        this.SetText(true);
    }


    private void SetText(bool changeTimeline)
    {
        this._StartTxt.text = (this._StartSlider.value * 100f).ToString("f2");
        this._DuraTxt.text = (this._DurationSlider.value * 100f).ToString("f2");

        if (changeTimeline)
        {
            _clip.start = this._StartSlider.value * 100f;
            _clip.duration = this._DurationSlider.value * 100f;
           PhoneTimeLineUi.me.OnClipValueChange(this._uid);
        }
    }

    public List<PropertyRenderData> propertyDatas = new List<PropertyRenderData>();
    public void SetDetails(TimelineClip clip, string uid,float duration)
    {
        this._uid = uid;
        _clip = clip;
        _totleDuratation = duration * 0.01f;

        this._DurationSlider.minValue = 0.033f * 0.01f;//写死了最小调整为一帧的长度
        this._DurationSlider.maxValue = _totleDuratation;//

        this._StartSlider.minValue = 0;
        this._StartSlider.maxValue = _totleDuratation;//

        this._StartSlider.SetValue((float)clip.start * 0.01f,false);
        this._DurationSlider.SetValue((float)clip.duration * 0.01f,false);

        this.SetText(false);

        this._StartSlider.onValueChanged.AddListener(this.OnValueChanged);
        this._DurationSlider.onValueChanged.AddListener(this.OnValueChanged);

        this.SetProperty(clip);
    }

    private void SetProperty(TimelineClip clip)
    {
        for (int i = 0; i < this._childItems.Count; ++i)
        {
            GameObject.Destroy(_childItems[i].gameObject);
        }
        _childItems.Clear();

        Type type = clip.asset.GetType();
        MemberInfo[] infos = type == null ? null : type.GetAllVariables();
        propertyDatas.Clear();
        if (infos != null)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                var data = new PropertyRenderData();
                data.CopyFromMemberInfo(infos[i], clip.asset);
                this.AddOneProperty(data);
            }
        }
    }

    private void AddOneProperty(PropertyRenderData data)
    {
        GameObject obj = GameObject.Instantiate(this._Templete.gameObject);
        obj.transform.SetParent(this._scrollInspector.content.transform, false);
        obj.SetActive(true);
        PropertyItem item = obj.GetComponent<PropertyItem>();
        item.Render(data);
        this._childItems.Add(item);
    }

}//end class