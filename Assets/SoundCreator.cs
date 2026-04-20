using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public static class SoundCreator
{
    public static void PlaySoundOneShot(EventReference sound, Vector3 position)
    {
        EventInstance pickedSoundInstance = RuntimeManager.CreateInstance(sound);
        pickedSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        pickedSoundInstance.start();
        pickedSoundInstance.release();
    }
}
