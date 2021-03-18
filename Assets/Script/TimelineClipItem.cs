using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;


//add by tiny 一个可以让游戏暂停恢复、逐帧执行的脚本
public class TimelineClipItem : MonoBehaviour
{
    public Button _clickBtn;
    public GameObject _selected;
    public Text _TrackNameTxt;
    private TimelineClip _clip;
    private string _uid;
    private float _rootWidth;
    private float _totleDuration;
    private UIDragHandler _drager;

    private void Awake()
    {
        this._drager = this.gameObject.AddComponent<UIDragHandler>();
        this._drager._beginCall = this.OnBeginDrag;
        this._drager._dragCall = this.OnDrag;
        this._drager._endCall = this.OnEndDrag;
        this._drager.SetEnable(false);
        _selected.gameObject.SetActive(false);
        this._clickBtn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        //TimelineEditView.me.OnClickTimelineClick(this._uid);
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (this._selected.gameObject.activeSelf)
        {
            float percent = eventData.delta.x / this._rootWidth * this._totleDuration;
            //TimelineClipTips.Ins.OnStartValueChange(percent);
        }
    }

    public void OnEndDrag(PointerEventData eventData) { }

    public void SetDetails(TimelineClip clip, string uid, float rootWidth,float duration)
    {
        this._totleDuration = duration * 0.01f;
        this._rootWidth = rootWidth;
        this._uid = uid;
        this._clip = clip;
        this._TrackNameTxt.text = clip.displayName;
        this.AdjustPostion();
    }

    public void AdjustPostion()
    {
        double start = _clip.start;
        double end = _clip.end;
        float clipWidth = (float)_clip.duration / this._totleDuration * this._rootWidth;

        //大小
        Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(clipWidth, size.y);

        //起始位置
        float leftX = -this._rootWidth / 2;
        float posX = leftX + (float)start / this._totleDuration * this._rootWidth + clipWidth / 2;
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, 0);
    }

    public void SetSelect(string guid)
    {
        bool isSelect = guid == this._uid;
        this._drager.SetEnable(isSelect);
        this._selected.gameObject.SetActive(isSelect);
    }

    public TimelineClip GetClip()
    {
        return this._clip;
    }

}//end class