using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Items.BossLoot.StarplateDrops;
using SpiritMod.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Sets.SwordsMisc.AlphaBladeTree
{
	public class Starblade : ModItem
	{
		private int charger;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starblade");
			Tooltip.SetDefault("'Harness the night sky'\nEvery third swing causes the blade to release multiple bright stars\nEach star explodes into homing star wisps");
			SpiritGlowmask.AddGlowMask(Item.type, "SpiritMod/Items/Sets/SwordsMisc/AlphaBladeTree/Starblade_Glow");
		}

		public override void SetDefaults()
		{
			Item.damage = 35;
			Item.useTime = 27;
			Item.useAnimation = 27;
			Item.DamageType = DamageClass.Melee;
			Item.width = 50;
			Item.height = 50;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 4;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.LightRed;
			Item.shootSpeed = 8;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.shoot = ModContent.ProjectileType<Starshock2>();
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Electric);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].velocity *= 0.1f;
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskUtils.DrawItemGlowMaskWorld(spriteBatch, Item, ModContent.Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
		{
			if (++charger >= 3)
			{
				for (int i = 0; i < 3; i++)
					Projectile.NewProjectile(source, position.X - 8, position.Y + 8, velocity.X + ((float)Main.rand.Next(-230, 230) / 100), velocity.Y + ((float)Main.rand.Next(-230, 230) / 100), ModContent.ProjectileType<Starshock2>(), damage / 2, knockback, player.whoAmI, 0f, 0f);
				
				charger = 0;
			}
			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.FallenStar, 5);
			recipe.AddIngredient(ModContent.ItemType<CosmiliteShard>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}