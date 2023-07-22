using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editor;
using Sandbox;
using TerrorTown;

namespace GoldDeagle;

[Library("ttt_weapon_goldendeagle"), HammerEntity]

// Title is what shows up in the Shop and Inventory, Category is used for where it is in the Hammer entity list, this isn't used for the shop
[Title("Golden Deagle"), Category("Weapons")]

// This tells the game we want this gun in the shop, and what category and price it is.
[DetectiveBuyable("Weapons", 1)]
[EditorModel("models/weapons/w_golden_deagle.vmdl")]
public class GoldenDeagle : Gun
{
	// rather than defining the gun in a resource (like many other sbox games), we do everything in code, it's a lot cleaner imo than managing multiple files
	// this file is heavily commented as it should serve as a example.

	// Paths to our models for this gun, also set the EditorModel attribute to the world model so if someone wants to use it in hammer it'll show up. 
	public override string ViewModelPath => "models/weapons/v_golden_deagle.vmdl";
	public override string WorldModelPath => "models/weapons/w_golden_deagle.vmdl";

	// This is the time between shooting
	public override float PrimaryAttackDelay => 0.6f;

	// Only 2 bullets, the gun spawns with a full clip set to this maximum by default
	public override int MaxPrimaryAmmo => 2;

	// if true, holding click fires immediantly again after the delay time
	public override bool Automatic => true;

	// benefits for code only weapons is you can setup this to do anything easily, we want it to act like a gun so we just can do everything we normally do, except deal damage
	public override void PrimaryAttack()
	{
		base.PrimaryAttack();
		// take some ammo away
		PrimaryAmmo -= 1;
		// 0 damage, handle damage in the OnBulletTraceHit function so we can specialise it to oour needs
		ShootBullet(0, 0.008f, 37);
		// play a sound, this specific one in the gamemodes files and isn't from this addon but you can use sounds from where ever
		PlaySound("deagle.shoot");
		// animate the player holding us to shoot or whatever
		(Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);
		// muzzleflash, bullet casing and etc
		ShootEffects();
		if (Game.IsClient)
		{
			// Call this function to add some recoil.
			DoViewPunch(6f);
		}
	}

	// here we can see if the the ShootBullet() trace hit, and if it did, what it hit
	public override void OnBulletTraceHit(TraceResult tr)
	{
		if (Owner is TerrorTown.Player ply && tr.Entity is TerrorTown.Player hitply)
		{
			if (hitply.Team is Traitor)
			{
				// manually deal damage, 
				var dmgInfo = DamageInfo.FromBullet(tr.HitPosition, Owner.AimRay.Forward * 37, 500)
					.WithWeapon(this)
					.WithBone(tr.Bone)
					.WithAttacker(Owner)
					.WithTag("bullet");
				hitply.TakeDamage(dmgInfo);
			} 
			else
			{
				// not a traitor, karma time lol
				var dmgInfo = DamageInfo.FromBullet(tr.HitPosition, Owner.AimRay.Forward * -37, 500)
					.WithWeapon(this)
					.WithBone(tr.Bone)
					.WithAttacker(Owner)
					.WithTag("bullet");
				ply.TakeDamage(dmgInfo);
			}
		}
	}
}
