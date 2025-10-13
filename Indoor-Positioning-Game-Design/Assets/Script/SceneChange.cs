using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void MoveScene (int SceneNo)
    {
        SceneManager.LoadScene(SceneNo);
    }
}
