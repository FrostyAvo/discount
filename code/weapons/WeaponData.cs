using Sandbox;

namespace Discount.Weapons
{
	[Library( "weapon" )]
	public class WeaponData : Asset
	{
		public string Name { get; set; } = "Weapon Name";
		public string ViewModelPath { get; set; } = "";
		public string WorldModelPath { get; set; } = "";
		public string PrimaryFireSound { get; set; } = "";
		public int HoldType { get; set; } = 0;
		public int BulletsPerShot { get; set; } = 1;
		public float PrimaryFireRate { get; set; } = 1;
		public float Damage { get; set; } = 5;
		public float Knockback { get; set; } = 0;
		public float SpreadAngle { get; set; } = 10;
		public float Range { get; set; } = 2000;
	}
}
