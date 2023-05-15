////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public interface IActions
{
    Texture FetchImage(string url);
    void SelectPage(string url);
}

