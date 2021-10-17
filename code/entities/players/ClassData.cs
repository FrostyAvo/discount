using Sandbox;

namespace Discount
{
	public enum ClassSpecialAbility : int { None, Buildings }

	[Library("class")]
	public class ClassData : Asset
	{
		public readonly static string[] AllClasses = new string[]
		{
			"scout", "soldier", "heavy", "engineer", "medic", "sniper"
		};

		public new string Name { get; set; } = "Class";
		public int Health { get; set; } = 125;
		public float MoveSpeed { get; set; } = 150;
		public ClassSpecialAbility SpecialAbility { get; set; } = ClassSpecialAbility.None;
		public string[] Weapons { get; set; } = new string[] { };
		public string Hat { get; set; } = "";
		public string RedShirt { get; set; } = "";
		public string BlueShirt { get; set; } = "";
		public string RedPants { get; set; } = "";
		public string BluePants { get; set; } = "";
		public string Shoes { get; set; } = "";

		public string[] ExtraClothes { get; set; } = new string[] { };
	}
}
