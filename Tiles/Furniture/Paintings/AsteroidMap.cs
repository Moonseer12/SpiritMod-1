using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
namespace SpiritMod.Tiles.Furniture.Paintings
{
	public class AsteroidMap : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;

			Terraria.ID.TileID.Sets.FramesOnKillWall[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.CoordinateHeights = new int[]
			{
				16,
				16,
				16
			};
			TileObjectData.newTile.AnchorBottom = default(AnchorData);
			TileObjectData.newTile.AnchorTop = default(AnchorData);
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.addTile(Type);
			DustType = -1;
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Map");
			AddMapEntry(new Color(150, 150, 150), name);
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
            Item.NewItem(new Terraria.DataStructures.EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<Items.Consumable.Quest.ExplorerScrollAsteroidFull>());
        }
	}
}