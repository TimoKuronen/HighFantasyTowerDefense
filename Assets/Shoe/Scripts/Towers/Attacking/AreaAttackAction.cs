using UnityEngine;

public class AreaAttackAction : ITowerAction
{
    public void Execute(TowerAI controller)
    {
        if (controller.TargetToShoot == null)
        {
            return;
        }

        var projectile = controller.TowerData.ProjectileToShoot.Get<Projectile>(
            controller.transform.position,
            Quaternion.identity);

        projectile.SetDamageData(controller.GetTowerDamageData());
        projectile.DealDamage(null);

        controller.CooldownTrigger();
    }
}