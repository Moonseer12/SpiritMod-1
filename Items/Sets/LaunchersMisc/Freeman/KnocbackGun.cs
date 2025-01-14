using Microsoft.Xna.Framework;
using SpiritMod.Projectiles.Bullet;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Sets.LaunchersMisc.Freeman
{
	public class KnocbackGun : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Freeman");
			Tooltip.SetDefault("Converts rockets fired into coiled rockets that can be controlled by the cursor\n'The right man in the wrong place can make all the difference in the world'");
		}

		public override void SetDefaults()
		{
			Item.damage = 23;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 65;
			Item.height = 21;
			Item.useTime = 55;
			Item.useAnimation = 55;
			Item.useTurn = false;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 7;
			Item.channel = true;
			Item.value = Item.buyPrice(0, 11, 0, 0);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item36;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<FreemanRocket>();
			Item.shootSpeed = 6f;
			Item.useAmmo = AmmoID.Rocket;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			Vector2 muzzleOffset = Vector2.Normalize(velocity) * 45f;
			if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
				position += muzzleOffset;

			SoundEngine.PlaySound(new SoundStyle("SpiritMod/Sounds/CoilRocket"), player.Center);
			type = ModContent.ProjectileType<FreemanRocket>();
		}
	}
}