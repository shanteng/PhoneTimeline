using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;


//add by tiny 一个可以让游戏暂停恢复、逐帧执行的脚本
public class PlayableBindingItem : MonoBehaviour
{
    public RectTransform _TrackRoot;
    public TimelineClipItem _templete;
    public Text _NameTxt;
    private TrackAsset _track;
    private float _Rootwidth;
    private string _uid;

    public Dictionary<string, TimelineClipItem> _Clips = new Dictionary<string, TimelineClipItem>();
    private void Awake()
    {
        this._templete.gameObject.SetActive(false);
    }

    public void SetDetails(TrackAsset track, string uid,PlayableDirector director)
    {
        this._uid = uid;
        this._Rootwidth = this._TrackRoot.sizeDelta.x;
        this._track = track;
        this._NameTxt.text = this._track.name;
        foreach (TimelineClip clip in track.GetClips())
        {
            this.CreateOneClip(clip, director);
        }
    }

    private void CreateOneClip(TimelineClip clip, PlayableDirector director)
    {
        //实例化
        GameObject obj = GameObject.Instantiate(this._templete.gameObject);
        obj.name = clip.displayName;
        obj.transform.SetParent(this._TrackRoot, false);
        obj.SetActive(true);

        string uid = Utils.GenerateUId();
        TimelineClipItem item = obj.GetComponent<TimelineClipItem>();
        item.SetDetails(clip, uid, this._Rootwidth,(float)director.duration);
        _Clips.Add(uid, item);
    }

}//end class