using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Projectiles.Arrow;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.DataStructures;

namespace SpiritMod.Items.BossLoot.DuskingDrops
{
	public class Shadowmoor : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shadowmoor");
            Tooltip.SetDefault("Converts wooden arrows into Shadow Wisps");
            SpiritGlowmask.AddGlowMask(Item.type, "SpiritMod/Items/BossLoot/DuskingDrops/Shadowmoor_Glow");
        }

		public override void SetDefaults()
		{
			Item.damage = 35;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 26;
			Item.height = 62;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<ShadowmoorProjectile>();
			Item.useAmmo = AmmoID.Arrow;
			Item.knockBack = 3.25f;
			Item.value = Item.sellPrice(0, 4, 0, 0);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item5;
			Item.autoReuse = true;
			Item.shootSpeed = 16.2f;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (type == ProjectileID.WoodenArrowFriendly)
			{
				type = ModContent.ProjectileType<ShadowmoorProjectile>();
				damage = (int)(damage * .75f);
			}
			else
			{
				float angle = Main.rand.NextFloat(MathHelper.PiOver4, -MathHelper.Pi - MathHelper.PiOver4);
				Vector2 spawnPlace = Vector2.Normalize(new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))) * 20f;

				if (Collision.CanHit(position, 0, 0, position + spawnPlace, 0, 0))
					position += spawnPlace;
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
		{
			if (type != ProjectileID.WoodenArrowFriendly)
			{ 
                DustHelper.DrawDiamond(new Vector2(position.X, position.Y), 173, 2, .8f, .75f);
                Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, ModContent.ProjectileType<ShadowmoorProjectile>(), damage, knockback, player.whoAmI);
            }

            for (int I = 0; I < 4; I++)
            {
                DustHelper.DrawDiamond(new Vector2(position.X, position.Y), 173, 2, .8f, .75f);
                Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);
            }
			return false;
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
			=> GlowmaskUtils.DrawItemGlowMaskWorld(spriteBatch, Item, ModContent.Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);
	}
}