using UnityEngine;

public class FixedResolution : MonoBehaviour
{
    private void Start()
    {
        SetResolution();
    }

    public void SetResolution()
    {
         int width = 1920;
        int height = 1080;
        bool fullscreen = true;
        //int width = 1600;
        //int height = 900;
        //bool fullscreen = false;

        Screen.SetResolution(width, height, fullscreen);
    }
}
