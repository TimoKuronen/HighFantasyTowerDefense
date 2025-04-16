﻿using System.Collections;
using UnityEngine;

public class FrozenEffect : IStatusEffect
{
    public StatusEffectData EffectData { get; set; }

    public FrozenEffect(StatusEffectData data)
    {
        EffectData = data;
    }

    public IEnumerator Tick(Health target)
    {
        float timer = EffectData.duration;

        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer -= 1;
        }

        target.RemoveEffect(this);
    }
}
