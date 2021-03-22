using UnityEngine;
using System.Collections;
using UnityEngine.Events;


//add by tiny 一个可以让游戏暂停恢复、逐帧执行的脚本
public class GameFramePause : MonoBehaviour
{
    private bool _wateFrame = false;
    public UnityAction<float> _OneFrameCallBack;
    private void Awake()
    {

    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        _wateFrame = false;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public void ActionOneFrame()
    {
        _wateFrame = true;
        Time.timeScale = 1;
        StartCoroutine(OneFrameTimeScale());
    }

    public IEnumerator OneFrameTimeScale()
    {
        while (_wateFrame)
        {
            yield return 0;
        }
        _wateFrame = false;
        Time.timeScale = 0;
    }
}//end class