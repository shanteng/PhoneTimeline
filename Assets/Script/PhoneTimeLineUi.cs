using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

public class PhoneTimeLineUi : MonoBehaviour
{
    private GameFramePause _pauseScript;
    public ScrollRect _scroll;
    public PlayableBindingItem _templete;
    private Dictionary<string, PlayableBindingItem> _Tracks = new Dictionary<string, PlayableBindingItem>();
    public Dictionary<string, TimelineClipItem> _AllClips = new Dictionary<string, TimelineClipItem>();

    public RectTransform _bottomRect;
    public Button _btnPre;
    public Button _btnPlay;
    public Button _btnPause;
    public Button _btnNext;

    public Button _btnHide;
    public Button _btnShow;
    private bool _isHide = false;
    public Vector2 HidePos = new Vector2(0, 280);

    public Text _curSecsTxt;
    public ClickSlider _Slider;

    public List<UITextWrapper> _timeLabels;
    public ClipTipsUi _ClipTips;

    private PlayableDirector _director;
    private bool _isPause = false;
    private bool _isAdjustSlider = false;
    private float FPS = 0;
    private string _selectUId = "";

    public static PhoneTimeLineUi me;
    void Awake()
    {
        me = this;
        this._pauseScript = this.gameObject.AddComponent<GameFramePause>();
        _templete.gameObject.SetActive(false);
        _btnPre.onClick.AddListener(OnPreFrameClick);
        _btnNext.onClick.AddListener(OnNextFrameClick);
        _btnPlay.onClick.AddListener(PlayTimeLine);
        _btnPause.onClick.AddListener(PauseTimeLine);
        this._Slider.onValueChanged.AddListener(this.OnSliderValueChanged);
        this._Slider.mOnPointerDown.AddListener(this.OnPointerDown);
        this._Slider.mOnPointerDown.AddListener(this.OnPointerUp);

        _btnHide.onClick.AddListener(OnClickHideShow);
        _btnShow.onClick.AddListener(OnClickHideShow);
    }


    private void OnClickHideShow()
    {
        this._isHide = !this._isHide;
        this.SetHideShowState();
    }

    private void SetHideShowState()
    {
        this._btnShow.gameObject.SetActive(this._isHide);
        this._btnHide.gameObject.SetActive(!this._isHide);
        Vector2 anPos = this._isHide ? this.HidePos : Vector2.zero;
        this._bottomRect.anchoredPosition = anPos;
    }

    private void Start()
    {
        this.Init();
    }

    protected Coroutine _timeCor;
    public void Init()
    {
        this._isHide = false;
        this.SetHideShowState();
        foreach (GameObject objj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
        {
            if (objj.GetComponent<PlayableDirector>() != null)
            {
                this._director = objj.GetComponent<PlayableDirector>();
                break;
            }
        }

        if (this._director == null)
            return;

        TimelineAsset timelineAsset = this._director.playableAsset as TimelineAsset;
        this.FPS = timelineAsset.editorSettings.fps;
        this._isAdjustSlider = false;
        this.PlayTimeLine();
        this.InitTracks();
        this.InitTotleSecs();
        this.SetSlider();
        this.HideClipTips();
        if (_timeCor != null)
        {
            StopCoroutine(_timeCor);
            this._timeCor = null;
        }
        _timeCor = StartCoroutine(TimelineCoroutine());
    }

    private void InitTracks()
    {
        foreach (PlayableBinding pb in this._director.playableAsset.outputs)
        {
            this.CreateOneTrack(pb);
        }
    }

    private void CreateOneTrack(PlayableBinding pb)
    {
        TrackAsset track = pb.sourceObject as TrackAsset;
        //实例化
        GameObject obj = GameObject.Instantiate(this._templete.gameObject);
        obj.name = track.name;
        obj.transform.SetParent(this._scroll.content.transform, false);
        obj.SetActive(true);
        string uid = Utils.GenerateUId();
        PlayableBindingItem item = obj.GetComponent<PlayableBindingItem>();
        item.SetDetails(track, uid, this._director);
        _Tracks.Add(uid, item);

        foreach (string id in item._Clips.Keys)
        {
            this._AllClips.Add(id, item._Clips[id]);
        }
    }

    private void InitTotleSecs()
    {
        float oneSecs = (float)this._director.duration / this._timeLabels.Count;
        for (int i = 0; i < this._timeLabels.Count; ++i)
        {
            float curSecs = (i + 1) * oneSecs;
            int frame = (int)(this.FPS * curSecs);
            _timeLabels[i]._Labels[0].text = frame.ToString();
        }
    }

    public IEnumerator TimelineCoroutine()
    {
        while (_director != null)
        {
            if (this._isPause == false && this._isAdjustSlider == false)
                this.SetSlider();
            yield return 0;
        }
    }

    public void PlayTimeLine()
    {
        this._isPause = false;
        this._pauseScript.ResumeGame();
        this._btnPause.gameObject.SetActive(true);
        this._btnPlay.gameObject.SetActive(false);
    }

    public void PauseTimeLine()
    {
        this._isPause = true;
        this._pauseScript.PauseGame();
        this._btnPause.gameObject.SetActive(false);
        this._btnPlay.gameObject.SetActive(true);
    }

    private void OnPreFrameClick()
    {
        this.CutSceneOneFrame(false);
    }

    private void OnNextFrameClick()
    {
        this.CutSceneOneFrame(true);
    }

    private void CutSceneOneFrame(bool forward)
    {
        if (this._director == null) return;

        _director.time = forward ? Mathf.Min((float)_director.duration, (float)_director.time + 1 / this.FPS) : Mathf.Max(0, (float)_director.time - 1 / this.FPS);
        this.SetSlider();
    }

    private void SetSlider()
    {
        if (this._director == null) return;
        float curProgress = (float)this._director.time / (float)this._director.duration;
        this._Slider.SetValue(curProgress, false);
        this.UpdateFrameTxt();
    }

    private void UpdateFrameTxt()
    {
        float frame = (float)(this.FPS * this._director.time);
        this._curSecsTxt.text = frame.ToString("f1");
    }

    private void OnSliderValueChanged(float value)
    {
        if (this._director == null) return;
        float curTime = (float)this._Slider.normalizedValue * (float)this._director.duration;
        if (curTime == this._director.time)
            return;
        this._director.time = curTime;
        this.UpdateFrameTxt();
    }

    private void OnPointerDown()
    {
        _isAdjustSlider = false;
    }

    private void OnPointerUp()
    {
        _isAdjustSlider = false;
    }

    private void HideClipTips()
    {
        this._ClipTips.gameObject.SetActive(false);
        this._selectUId = "";
    }

    private void ShowClickTips(TimelineClip clip,string uid)
    {
        this._selectUId = uid;
        this._ClipTips.gameObject.SetActive(true);
        this._ClipTips.SetDetails(clip, this._selectUId,(float)this._director.duration);
    }

  
    public void OnClickTimelineClick(string uid)
    {
        bool isSame = this._selectUId == uid;
        if (isSame)
        {
            this.HideClipTips();
        }
        else
        {
            TimelineClipItem itemCur = null;
            if (this._AllClips.TryGetValue(uid, out itemCur))
            {
                this.ShowClickTips(itemCur.GetClip(),uid);
            }
        }

        foreach (TimelineClipItem item in this._AllClips.Values)
        {
            item.SetSelect(_selectUId);
        }
    }//end func

    public void OnClipValueChange(string uid)
    {
        TimelineClipItem itemCur = null;
        if (this._AllClips.TryGetValue(uid, out itemCur))
        {
            itemCur.AdjustPostion();
        }
    }

}//end class