using Terraria.ID;
using Terraria.ModLoader;
using NeonMusicBoxTile = SpiritMod.Tiles.MusicBox.NeonMusicBox;

namespace SpiritMod.Items.Placeable.MusicBox
{
	[Sacrifice(1)]
	public class NeonMusicBox : ModItem
	{
		public override void SetStaticDefaults() => DisplayName.SetDefault("Music Box (Hyperspace- Day)");

		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<NeonMusicBoxTile>();
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.LightRed;
			Item.value = 100000;
			Item.accessory = true;
			Item.canBePlacedInVanityRegardlessOfConditions = true;
		}
    }
}
