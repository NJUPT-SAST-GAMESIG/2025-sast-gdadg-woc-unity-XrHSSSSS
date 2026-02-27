using UnityEngine;


namespace Character.Player
{
    public class PlayerSoundControl : MonoBehaviour
    {
        [Header("Footstep Clips")]
        [SerializeField] private AudioClip[] footstepClips;

        [Header("RunStop Clips")]
        [SerializeField] private AudioClip[] runStopClips;

        [Header("Weapon Sound Clips")]
        [SerializeField] private AudioClip[] weaponBackClips;
        [SerializeField] private AudioClip[] weaponEndClips;

        [Header("Audio Settings")]
        [SerializeField] private float runVolume = 1f;
        [SerializeField] private float walkVolume = 0.8f;
        [SerializeField] private float runStopVolume = 1f;
        [SerializeField] private float weaponBackVolume = 1f;
        [SerializeField] private float weaponEndVolume = 1f;
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void LFootRun()
        {
            PlayClipFromPool("LFootRun", footstepClips, runVolume);
        }

        public void RFootRun()
        {
            PlayClipFromPool("RFootRun", footstepClips, runVolume);
        }

        public void LFootWalk()
        {
            PlayClipFromPool("LFootWalk", footstepClips, walkVolume);
        }

        public void RFootWalk()
        {
            PlayClipFromPool("RFootWalk", footstepClips, walkVolume);
        }

        public void RunStop()
        {
            PlayClipFromPool("RunStop", runStopClips, runStopVolume);
        }


        private void PlayClipFromPool(string eventName, AudioClip[] clipPool, float volume)
        {


            AudioClip clip = GetRandomClip(clipPool);

            //Debug.Log($"SoundEvent: {eventName}, clip: {clip.name}", this);

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip, volume);
                return;
            }



            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }

        private AudioClip GetRandomClip(AudioClip[] clipPool)
        {
            int randomIndex = Random.Range(0, clipPool.Length);
            return clipPool[randomIndex];
        }

        private void PlayWeaponBackSound()
        {
            PlayClipFromPool("PlayWeaponBackSound", weaponBackClips, weaponBackVolume);
        }



        private void PlayWeaponEndSound()
        {
            PlayClipFromPool("PlayWeaponEndSound", weaponEndClips, weaponEndVolume);
        }

    }

}
