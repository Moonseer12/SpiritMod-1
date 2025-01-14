using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.BossLoot.StarplateDrops
{
	public class Glowstone : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlendAll[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileOreFinderPriority[Type] = 300;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Astralite");
			AddMapEntry(new Color(156, 102, 36), name);

			ItemDrop = ModContent.ItemType<CosmiliteShard>();
			HitSound = SoundID.Tink;
			DustType = DustID.Electric;
		}

		public override void NearbyEffects(int i, int j, bool closer) => MinPick = MyWorld.downedRaider ? 0 : 10000; //STUPID workaround for blockswap but it has to happen

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			Player player = Main.LocalPlayer;
			if (Vector2.DistanceSquared(new Vector2(i * 16, j * 16), player.Center) < 54 * 54)
				SoundEngine.PlaySound(SoundID.NPCHit4);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.039f * 2;
			g = .1f * 2;
			b = .1275f * 2;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
			int height = tile.TileFrameY == 36 ? 18 : 16;
			Main.spriteBatch.Draw(Mod.Assets.Request<Texture2D>("Items/BossLoot/StarplateDrops/Glowstone_Glow").Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height), new Color(100, 100, 100), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => MyWorld.downedRaider;
		public override bool CanExplode(int i, int j) => MyWorld.downedRaider;
	}
}