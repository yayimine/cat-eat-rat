using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    private VideoPlayer player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<VideoPlayer>();
        Debug.Log("Got ye");
    }

    public void PlayVideo()
    {
        player.Play();
        Debug.Log("PALY");
    }

    public void StopVideo()
    {
        player.Stop();
        Debug.Log("Stopp");
    }
    
    
}
