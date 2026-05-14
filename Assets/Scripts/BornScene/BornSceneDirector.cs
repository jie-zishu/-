using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class BornSceneDirector : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform sitPosition;

    private PlayableDirector director;
    private Animator animator;
    private CharacterController cc;
    private bool hasPlayed;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnTimelineStopped;
    }

    private void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnTimelineStopped;
    }

    /// <summary>
    /// Public entry point — call this instead of director.Play() directly.
    /// </summary>
    public void PlayTimeline()
    {
        hasPlayed = true;
        director.Play();
    }

    /// <summary>
    /// Called from Timeline Signal at 0s.
    /// </summary>
    public void PreparePlayerForSit()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogError("[BornSceneDirector] Player not found!"); return; }
        if (sitPosition == null) { Debug.LogError("[BornSceneDirector] Sit Position not assigned!"); return; }

        // Try to find the Animator on the armature child if not already cached
        if (animator == null)
        {
            var arm = player.transform.Find("PlayerArmature");
            if (arm != null) animator = arm.GetComponent<Animator>();
        }
        if (cc == null) cc = player.GetComponentInChildren<CharacterController>();

        Debug.Log("[BornSceneDirector] PreparePlayerForSit called. animator=" + (animator != null) + " cc=" + (cc != null));

        if (cc != null) cc.enabled = false;
        player.transform.position = sitPosition.position;
        player.transform.rotation = sitPosition.rotation;

        var armature = player.transform.Find("PlayerArmature");
        if (armature != null)
        {
            armature.localPosition = Vector3.zero;
            armature.localRotation = Quaternion.identity;
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetTrigger("SitDown_Sleep");
            Debug.Log("[BornSceneDirector] SitDown_Sleep trigger sent.");
        }
        else
        {
            Debug.LogError("[BornSceneDirector] No Animator found on Player or its children!");
        }
    }

    private void OnTimelineStopped(PlayableDirector d)
    {
        if (!hasPlayed) return;
        GameFrameworkManager.Instance.CompleteLevel(0);
    }
}
