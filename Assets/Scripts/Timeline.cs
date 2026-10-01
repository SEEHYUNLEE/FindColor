using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수

public class Timeline : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private PlayableDirector director;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void OnEnable()
    {
        director.stopped += OnTimelineEnd;
    }

    void OnDisable()
    {
        // 에러 방지, 이벤트 연결 해제.
        director.stopped -= OnTimelineEnd;
    }

    void OnTimelineEnd(PlayableDirector pd)
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
