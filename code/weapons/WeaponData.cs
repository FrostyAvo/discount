using Sandbox;

namespace Discount.Weapons
{
	[Library( "weapon" )]
	public class WeaponData : Asset
	{
		public new string Name { get; set; } = "Weapon";
		public string ViewModelPath { get; set; } = "";
		public string WorldModelPath { get; set; } = "";
		public int AttackType { get; set; } = 0;
		public string PrimaryFireSound { get; set; } = "";
		public int HoldType { get; set; } = 0;
		public int BulletsPerShot { get; set; } = 1;
		public float PrimaryFireRate { get; set; } = 1;
		public int AmmoPerShot { get; set; } = 1;
		public int ClipSize { get; set; } = 1;
		public int ReserveAmmo { get; set; } = 1;
		public int AmmoPerReload { get; set; } = 1;
		public float ReloadTime { get; set; } = 1f;
		public string ReloadSound { get; set; } = "";
		public float Damage { get; set; } = 5;
		public float Knockback { get; set; } = 0;
		public float SpreadAngle { get; set; } = 10;
		public float Range { get; set; } = 2000;
		public float DeployTime { get; set; } = 0;
		public bool CanHeadshot { get; set; } = false;
		public float WindupTime { get; set; } = 0;
		public string WindupSound { get; set; } = "";
		public bool GravityAffected { get; set; } = false;
		public bool Explosive { get; set; } = false;
		public float ExplosionRadius { get; set; } = 100f;
		public bool DisarmAfterFirstHit { get; set; } = false;
		public bool Sticky { get; set; } = false;
		public bool EmitSmoke { get; set; } = false;
		public float ProjectileSpeed { get; set; } = 1000f;
		public float ProjectileLifetime { get; set; } = 10f;
	}
}
