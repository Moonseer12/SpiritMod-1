using SpiritMod.NPCs.Boss.Atlas;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Consumable
{
	public class StoneSkin : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Stone Fist");
			Tooltip.SetDefault("Use anywhere to summon Atlas");
		}

		public override void SetDefaults()
		{
			Item.width = Item.height = 16;
			Item.rare = ItemRarityID.Cyan;
			Item.maxStack = 99;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = Item.useAnimation = 20;
			Item.noMelee = true;
			Item.consumable = false;
			Item.autoReuse = false;
			Item.UseSound = SoundID.Item43;
		}

		public override bool CanUseItem(Player player)
		{
			if (!NPC.AnyNPCs(ModContent.NPCType<Atlas>()))
				return true;
			return false;
		}

		public override bool? UseItem(Player player)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Atlas>());
			else if (Main.netMode == NetmodeID.MultiplayerClient && player == Main.LocalPlayer)
				SpiritMultiplayer.SpawnBossFromClient((byte)player.whoAmI, ModContent.NPCType<Atlas>(), (int)(int)player.Center.X, (int)(int)player.Center.Y - 600);

			SoundEngine.PlaySound(SoundID.Roar, player.Center);

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.LihzahrdPowerCell, 1);
			recipe.AddIngredient(ItemID.MartianConduitPlating, 20);
			recipe.AddIngredient(ItemID.StoneBlock, 100);
			recipe.AddIngredient(ItemID.Bone, 10);
			recipe.Register();
		}
	}
}
