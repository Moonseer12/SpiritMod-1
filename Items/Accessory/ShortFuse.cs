using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.GlobalClasses.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Accessory
{
	[Sacrifice(1)]
	public class ShortFuse : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Short Fuse");
			Tooltip.SetDefault("Explosives burn quicker\nWorks while in the inventory");
		}

		public override void SetDefaults()
		{
			Item.Size = new Vector2(18, 26);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 30, 0);
		}

		public override void UpdateInventory(Player player)
		{
			for (int i = 0; i < Main.maxProjectiles; ++i) 
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && ExplosivesCache.AllExplosives.Contains(p.type) && p.timeLeft % 12 == 0)
					p.timeLeft -= 5; //5/12ths faster I think. Or 7/12ths. idk
			} 
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) =>
			GlowmaskUtils.DrawItemGlowMaskWorld(spriteBatch, Item, ModContent.Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);
	}
}
