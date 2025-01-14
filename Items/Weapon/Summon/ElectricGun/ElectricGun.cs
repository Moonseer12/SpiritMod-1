using SpiritMod.Projectiles.Summon;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace SpiritMod.Items.Weapon.Summon.ElectricGun
{
	public class ElectricGun : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Arcbolter");
			Tooltip.SetDefault("Your summons will focus struck enemies\nHit enemies may create static links between each other when struck by minions, dealing additional damage");
            SpiritGlowmask.AddGlowMask(Item.type, "SpiritMod/Items/Weapon/Summon/ElectricGun/ElectricGun_Glow");
        }

        public override void SetDefaults()
		{
			Item.damage = 12;
			Item.DamageType = DamageClass.Summon;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 2;
			Item.value = Item.sellPrice(0, 0, 80, 0);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item12;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<ElectricGunProjectile>();
			Item.shootSpeed = 12f;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y - 1)) * 45f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
                position += muzzleOffset;

			velocity = velocity.RotatedByRandom(MathHelper.ToRadians(10));
            Projectile.NewProjectileDirect(source, position, velocity, type, Item.damage, knockback, player.whoAmI);

			for (int index1 = 0; index1 < 5; ++index1)
            {
                int index2 = Dust.NewDust(new Vector2(position.X, position.Y), Item.width - 60, Item.height - 28, DustID.Electric, velocity.X, velocity.Y, (int)byte.MaxValue, new Color(), Main.rand.Next(6, 10) * 0.1f);
                Main.dust[index2].noGravity = true;
                Main.dust[index2].velocity *= 0.5f;
                Main.dust[index2].scale *= .6f;
            }
            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-7, 0);

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			Lighting.AddLight(Item.position, 0.08f, .4f, .28f);
			GlowmaskUtils.DrawItemGlowMaskWorld(spriteBatch, Item, ModContent.Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);
		}
	}
}
