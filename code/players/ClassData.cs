using Sandbox;

namespace Discount
{
	[Library("class")]
	public class ClassData : Asset
	{
		public new string Name { get; set; } = "Class";
		public int Health { get; set; } = 125;
		public float MoveSpeed { get; set; } = 150;
		public string PrimaryWeapon { get; set; } = "";
		public string SecondaryWeapon { get; set; } = "";
		public string MeleeWeapon { get; set; } = "";
		public string HatPath { get; set; } = "";
		public string RedShirtPath { get; set; } = "";
		public string BlueShirtPath { get; set; } = "";
		public string RedPantsPath { get; set; } = "";
		public string BluePantsPath { get; set; } = "";
		public string ShoesPath { get; set; } = "";
	}
}
