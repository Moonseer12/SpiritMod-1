using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Projectiles.Summon.MoonjellySummon;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.BossLoot.MoonWizardDrops
{
	public class MoonjellySummonStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lunazoa Staff");
			Tooltip.SetDefault("Summons a Moonlight Preserver\nMoonlight Preservers summon smaller jellyfish that explode\nOnly one Moonlight Preserver can exist at once\nUsing the staff multiple times takes up summon slots, but increases jellyfish spawn rates");
			SpiritGlowmask.AddGlowMask(Item.type, Texture + "_Glow");
		}

		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 38;
			Item.value = Item.sellPrice(0, 2, 30, 0);
			Item.rare = ItemRarityID.Green;
			Item.mana = 10;
			Item.damage = 16;
			Item.knockBack = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.DamageType = DamageClass.Summon;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<MoonjellySummon>();
			Item.UseSound = SoundID.Item44;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
				player.MinionNPCTargetAim(true);
			else if (FindSummon(player, out Projectile summon))
				summon.minionSlots++;

			return base.UseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
			=> player.altFunctionUse != 2 && !FindSummon(player, out _);

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			Lighting.AddLight(Item.position, 0.08f, .4f, .28f);
			Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			GlowmaskUtils.DrawItemGlowMaskWorld(spriteBatch, Item, texture, rotation, scale);
		}

		private bool FindSummon(Player player, out Projectile projectile)
		{
			var found = Main.projectile.Where(x => x.active && x.owner == player.whoAmI && x.type == Item.shoot);

			if (found.Any())
			{
				projectile = found.First();
				return true;
			}
			else
			{
				projectile = null;
				return false;
			}
		}
	}
}